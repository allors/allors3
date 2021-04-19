npx create-nx-workspace@latest allors --preset=empty --cli=nx --nx-cloud=false

cd allors

npm install -D jest-chain
npm install -D jest-extended

npx nx g @nrwl/workspace:library workspace/system

npx nx g @nrwl/workspace:library workspace/lazy
npx nx g @nrwl/workspace:library tests/workspace/lazy
