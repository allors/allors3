rm -rf modules

npx create-nx-workspace@latest modules --preset=empty --cli=nx --nx-cloud=false

cd modules

npm install -D jest-trx-results-processor

npm install cross-fetch

// Core
npx nx g @nrwl/workspace:library core/workspace/derivations
npx nx g @nrwl/workspace:library core/workspace/derivations-test
npx nx g @nrwl/workspace:library core/workspace/domain
npx nx g @nrwl/workspace:library core/workspace/meta
npx nx g @nrwl/workspace:library core/workspace/meta-json

// System
npx nx g @nrwl/workspace:library system/common/protocol-json

npx nx g @nrwl/workspace:library system/workspace/adapters
npx nx g @nrwl/workspace:library system/workspace/adapters-tests
npx nx g @nrwl/workspace:library system/workspace/adapters-json
npx nx g @nrwl/workspace:library system/workspace/adapters-json-tests

npx nx g @nrwl/workspace:library system/workspace/domain
npx nx g @nrwl/workspace:library system/workspace/meta
npx nx g @nrwl/workspace:library system/workspace/meta-tests
npx nx g @nrwl/workspace:library system/workspace/meta-json
npx nx g @nrwl/workspace:library system/workspace/meta-json-tests
