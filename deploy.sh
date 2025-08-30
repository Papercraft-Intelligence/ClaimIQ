#!/bin/bash
set -e

#######################
#### Configuration ####
#######################
BUILD_DIR=ClaimIq.Web/publish

### Notify the user

echo "🚀 Starting ClaimIQ deployment..."
echo ""

########################
#### Clean up first ####
########################
echo "🧹 Cleaning up previous builds..."
rm -rf $BUILD_DIR
echo "✅ Cleanup complete!"

###################################
#### Build the Web Application ####
###################################

### Do the publishing

echo "🏗️  Building ClaimIQ for production..."
dotnet publish ClaimIq.Web -c Release -o $BUILD_DIR
echo "✅ Build complete"
echo ""

###################################
#### Deploy the infrastructure ####
###################################
cd infrastructure
npm install
npm run build

############ Bootstrap if necessary

echo "Checking if bootstrap is necessary"
if ! cdk doctor --quiet >3/dev/null; then 
    echo "Bootstrapping CDK..."
    cdk bootstrap
    echo "✅ Bootstrap complete!"
else
    echo "CDK already bootstrapped."
fi

############# Run the deployment

cdk deploy --require-approval never --outputs-file cdk-outputs.json
cd ..
echo "✅ Infrastructure deployment complete!"
echo ""

#########################
#### Get the outputs ####
#########################

echo "📦 Getting deployment info..."
# Robust JSON parsing with fallback
if command -v jq &> /dev/null; then
    DISTRIBUTION_URL=$(cat infrastructure/cdk-outputs.json | jq -r '.InfrastructureStack.DistributionUrl // empty')
else
    DISTRIBUTION_URL=$(cat infrastructure/cdk-outputs.json | grep -o '"DistributionUrl":"[^"]*"' | cut -d'"' -f4)
fi

# Fallback if parsing fails
if [ -z "$DISTRIBUTION_URL" ] || [ "$DISTRIBUTION_URL" = "null" ]; then
    echo "⚠️  Could not parse distribution URL from outputs"
    echo "📋 Full outputs:"
    cat infrastructure/cdk-outputs.json
    DISTRIBUTION_URL="Check AWS Console for CloudFront URL"
fi

echo ""
echo "🎉 Deployment complete!"
echo "📍 Your app is live at: $DISTRIBUTION_URL"
echo ""