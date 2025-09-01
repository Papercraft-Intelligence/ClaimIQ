import * as cdk from 'aws-cdk-lib';
import * as cloudfront from 'aws-cdk-lib/aws-cloudfront'
import * as origins from 'aws-cdk-lib/aws-cloudfront-origins'
import * as s3deploy from 'aws-cdk-lib/aws-s3-deployment';
import * as s3 from 'aws-cdk-lib/aws-s3';
import * as lambda from 'aws-cdk-lib/aws-lambda';
import * as apigateway from 'aws-cdk-lib/aws-apigateway';
import { BlockPublicAccess } from 'aws-cdk-lib/aws-s3';
import { Construct } from 'constructs';
// import * as sqs from 'aws-cdk-lib/aws-sqs';

export class ClaimIqStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
      super(scope, id, props);


      //////////// Website Bucket ////////////
      const websiteBucket = new s3.Bucket(this, 'ClaimIqWebsiteBucket', {
        bucketName: `claimiq-web-${this.account}-${this.region}`,
        websiteIndexDocument: 'index.html',                                           // this is crucial for single-page applications
        websiteErrorDocument: 'index.html',                                           // serve index.html for all errors since it's a SPA
        publicReadAccess: true,                                                       // typical for static website hosting
        blockPublicAccess: BlockPublicAccess.BLOCK_ACLS_ONLY,                         // prevent ACL-based public access, since ACL is deprecated and bucket policies should be used instead
        removalPolicy: cdk.RemovalPolicy.DESTROY,                                     // only because this is a demo, normally you should use RETAIN because you don't want to lose your data
        autoDeleteObjects: true                                                       // required for DESTROY policy to work (apparently)
      });

      /////////// Lambda Function ////////////
      const claimsLambda = new lambda.Function(this, 'ClaimIqFunction', {
        runtime: lambda.Runtime.DOTNET_8,                                               // the runtime environment for the Lambda function
        memorySize: 256,                                                                // the amount of memory allocated to the function
        timeout: cdk.Duration.seconds(30),                                              // maximum execution time for the function
        architecture: lambda.Architecture.X86_64,                                       // architecture type
        code: lambda.Code.fromAsset('../ClaimIq.Api/publish'),                                  // location of the Lambda function code
        handler: 'ClaimIq.Api'                                                          // the function handler
      });
                  // example environment variable

      /////////// API Gateway ///////////////
      const claimsIqGateway = new apigateway.RestApi(this, 'ClaimIqApiGateway', {       // Generic gateway that can front for multiple services
        restApiName: 'ClaimIQ API Gateway',                                             // Generically named to encourage reuse
        description: 'Main API Gateway for ClaimIQ Services',                           // Friendly description
      });

      claimsIqGateway.root.addMethod('GET', new apigateway.MockIntegration({            // Define some behavior for the root `/` route for service discovery etc.
        integrationResponses: [{
          statusCode: '200',
          responseTemplates: {
            'application/json': JSON.stringify({
              name: 'ClaimIQ API Gateway',
              version: '1.0',
              message: 'Welcome to ClaimIQ API',
              endpoints: {
                'API Discovery': '/api',
                'Claims Service': '/api/claims'
              }
            })
          }
        }],
        requestTemplates: {
          'application/json': '{"statusCode": 200}'
        }
      }), {
        methodResponses: [{
          statusCode: '200',
          responseParameters: {
            'method.response.header.Content-Type': false
          }
        }]
      });

      ////////// Gateway Integrations /////////////
      const apiRootResource = claimsIqGateway.root.addResource('api');                  // handles /api/
      apiRootResource.addMethod('GET', new apigateway.MockIntegration({                 // Return a mock response at /api for service discovery, etc.
        integrationResponses: [
          {
            statusCode: '200',
            responseTemplates: {
              'application/json': JSON.stringify({ 
                message: 'ClaimIQ API Gateway',
                version: '1.0',
                services: ['/api/claims'],
                documentation: '/swagger'
              })
            }
          }
        ],
        requestTemplates: {
          'application/json': '{"statusCode": 200}'
        }
      }), {
        methodResponses:[{
          statusCode: '200'
        }]
      });

      // CLAIMS Resources
      const claimsIntegration = new apigateway.LambdaIntegration(claimsLambda);         // Integration for claims Lambda
      const claimsResource = apiRootResource.addResource('claims');                     // handles /api/claims
      claimsResource.addMethod('ANY', claimsIntegration);                               // Handles: /api/claims and /api/claims/{anything}
      claimsResource.addResource('{proxy+}').addMethod('ANY', claimsIntegration);       // Handles: /api/claims/CLM-123 and /api/claims/anything/else 

      

      //////////// CloudFront Distribution ////////////
      const distribution = new cloudfront.Distribution(this, 'ClaimIqDistribution', {
        defaultBehavior: {
          origin: new origins.S3StaticWebsiteOrigin(websiteBucket),                   // Cloudfront is like a photocopier for S3
          compress: true,                                                             // gzip when serving files for faster downloads
          allowedMethods: cloudfront.AllowedMethods.ALLOW_GET_HEAD_OPTIONS,           // only allow read operations since this is a static website
          viewerProtocolPolicy: cloudfront.ViewerProtocolPolicy.REDIRECT_TO_HTTPS,    // enforce HTTPS
          cachePolicy: cloudfront.CachePolicy.CACHING_OPTIMIZED,                      // use AWS recommended caching policy
        },
        additionalBehaviors: {
          '/api/*': {
            origin: new origins.RestApiOrigin(claimsIqGateway),                                 // forward /api/* requests to the API Gateway
            allowedMethods: cloudfront.AllowedMethods.ALLOW_ALL,                                // allow all methods for API requests
            cachePolicy: cloudfront.CachePolicy.CACHING_DISABLED,                               // disable caching for API requests
            viewerProtocolPolicy: cloudfront.ViewerProtocolPolicy.REDIRECT_TO_HTTPS,            // enforce HTTPS
            originRequestPolicy: cloudfront.OriginRequestPolicy.ALL_VIEWER_EXCEPT_HOST_HEADER   // forward all headers except Host
          }
        },
        defaultRootObject: 'index.html',                                              // this is crucial for single-page applications
        errorResponses: [
          {
            httpStatus: 404,                                                          // catch all 404 errors
            responseHttpStatus: 200,                                                  // serve back index with 200 to allow the SPA to handle the routing
            responsePagePath: '/index.html'                                           // serve index.html for all 404 errors
          }
        ]
        });

        // Under the hood, the BucketDeployment...
        // - creates a Lambda function that handles the file upload
        // - uploads the files to the S3 bucket
        // - packages files into a zip file
        // - executes the lambda to deploy the website
        // - invalidates the CloudFront cache

        new s3deploy.BucketDeployment(this, 'DeployWebsite', {                        // Name for the bucket to find it in a haystack
          sources: [s3deploy.Source.asset('../ClaimIq.Web/publish/wwwroot')],         // What to upload
          destinationBucket: websiteBucket,                                           // Where to upload it
          distribution,                                                               // CloudFront distribution to invalidate
          distributionPaths: ['/*'],                                                  // Paths to invalidate
        });


        //////////// CDK Outputs ////////////
        // CDK Output are like return values from the infrastructure deployment operations.
        // The CDK creates unfriendly CloudFormation outputs
        // This makes them passable to scripts and such

        new cdk.CfnOutput(this, 'BucketName', {                                         // Looks like claimiq-web-123456789-us-east-1
          value: websiteBucket.bucketName,                                              // Used by the deployment script to upload files
          description: 'S3 Bucket Name',
          });

        new cdk.CfnOutput(this, 'DistributionUrl', {                                    // Looks like https://d1234567890.cloudfront.net
          value: `https://${distribution.domainName}`,                                  // Used by the deployment script to echo back the URL of the live app
          description: 'CloudFront Distribution URL',                                   
        });                                                                           

        new cdk.CfnOutput(this, 'DistributionId', {                                      // Looks like d1234567890
          value: distribution.distributionId,                                            // This id can be used to invalidate the CloudFront cache
          description: 'CloudFront Distribution ID',                                     // The deployment script uses this for cache invalidation
        })

        new cdk.CfnOutput(this, 'ApiGatewayUrl', {                                      // Looks like https://d1234567890.execute-api.us-east-1.amazonaws.com/prod
          value: claimsIqGateway.url,
          description: 'Claims API Gateway URL',                                        
        });

  }
}
