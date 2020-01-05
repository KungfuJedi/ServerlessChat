cd ServerlessChat/ServerlessChatUI
npm install
npm run build.prod
aws s3 sync ./dist/ServerlessChatUI/ s3://kungfujedi-serverless-chat-ui --delete --acl public-read