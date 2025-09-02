.PHONY: dev clean kill build deploy verify

dev:
	@chmod +x dev.sh && ./dev.sh

clean:
	@echo "🧹 Cleaning .NET projects..."
	@dotnet clean
	@rm -rf ClaimIq.Api/publish ClaimIq.Web/publish


kill:
	@echo "🛑 Killing all ClaimIQ processes..."
	@pkill -f "ClaimIq" 2>/dev/null || true
	@pkill -f "dotnet watch" 2>/dev/null || true
	@lsof -ti:5001,5234 | xargs kill -9 2>/dev/null || true
	@echo "✅ All processes killed!"

build:
	@echo "🔨 Building for AWS deployment..."
	@make clean
	@dotnet restore
	@echo "📦 Publishing API for Lambda..."
	@cd ClaimIq.Api && dotnet publish -c Release -o publish --runtime linux-x64 --self-contained false
	@echo "🎨 Publishing Web for S3..."
	@cd ClaimIq.Web && dotnet publish -c Release -o publish
	@echo "✅ Build outputs ready for CDK!"

deploy:
	@echo "🚀 Deploying ClaimIQ to AWS..."
	@make build
	@echo "📦 Installing CDK dependencies..."
	@cd infrastructure && npm install
	@echo "🔧 Building CDK..."
	@cd infrastructure && npm run build
	@echo "🚀 Deploying with CDK..."
	@cd infrastructure && npx cdk deploy --require-approval never
	@echo "✅ Deployment complete!"

verify:
	@echo "🔍 Verifying build outputs..."
	@test -d ClaimIq.Api/publish || (echo "❌ API publish missing" && exit 1)
	@test -d ClaimIq.Web/publish/wwwroot || (echo "❌ Web publish missing" && exit 1)
	@echo "✅ All build outputs verified!"