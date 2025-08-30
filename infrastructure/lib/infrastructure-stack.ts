import * as cdk from 'aws-cdk-lib';
import * as cloudfront from 'aws-cdk-lib/aws-cloudfront'
import * as origins from 'aws-cdk-lib/aws-cloudfront-origins'
import * as s3deploy from 'aws-cdk-lib/aws-s3-deployment';
import * as s3 from 'aws-cdk-lib/aws-s3';
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
        autoDeleteObjects: true                                                       // required for DESTROY policy to work
      });

      //////////// CloudFront Distribution ////////////
      const distribution = new cloudfront.Distribution(this, 'ClaimIqDistribution', {
        defaultBehavior: {
          origin: new origins.S3StaticWebsiteOrigin(websiteBucket),                   // Cloudfront is like a photocopier for S3
          compress: true,                                                             // gzip when serving files for faster downloads
          allowedMethods: cloudfront.AllowedMethods.ALLOW_GET_HEAD_OPTIONS,           // only allow read operations since this is a static website
          viewerProtocolPolicy: cloudfront.ViewerProtocolPolicy.REDIRECT_TO_HTTPS,    // enforce HTTPS
          cachePolicy: cloudfront.CachePolicy.CACHING_OPTIMIZED,                      // use AWS recommended caching policy
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


        // Automatically deploy to s3
        new s3deploy.BucketDeployment(this, 'DeployWebsite', {
          sources: [s3deploy.Source.asset('../ClaimIq.Web/publish/wwwroot')],
          destinationBucket: websiteBucket,
          distribution,
          distributionPaths: ['/*'],
        });


        //////////// CDK Outputs ////////////
        // CDK Output are like return values from the infrastructure definition.
        // The CDK creates unfriendly CloudFormation outputs that are not easy to use in scripts.
        // This is a workaround to make them more user-friendly.

        new cdk.CfnOutput(this, 'BucketName', {                                         // Looks like claimiq-web-123456789-us-east-1
          value: websiteBucket.bucketName,                                              // Used by the deployment script to upload files
          description: 'S3 Bucket Name',
          });

        new cdk.CfnOutput(this, 'DistributionUrl', {                                    // Looks like https://d1234567890.cloudfront.net
          value: `https://${distribution.domainName}`,                                  // Used by the deployment script to echo back the URL of the live app
          description: 'CloudFront Distribution URL',                                   // Useful for validating the deployment
        });                                                                             // Later, might be set to a custom domain

        new cdk.CfnOutput(this, 'DistributionId', {                                      // Looks like d1234567890
          value: distribution.distributionId,                                            // This id can be used to invalidate the CloudFront cache
          description: 'CloudFront Distribution ID',                                     // The deployment script uses this for cache invalidation
        })

  }
}
