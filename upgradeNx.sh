npx create-nx-workspace@latest allors --preset=empty --cli=nx --nx-cloud=false

cd allors

rem npm install -D @nrwl/angular
npm install -D jest-chain
npm install -D jest-extended

npx nx g @nrwl/workspace:library shared/system
npx nx g @nrwl/workspace:library protocol/json/system

npx nx g @nrwl/workspace:library workspace/meta/system
npx nx g @nrwl/workspace:library workspace/meta/lazy/system
npx nx g @nrwl/workspace:library workspace/meta/core
npx nx g @nrwl/workspace:library workspace/meta/generated
npx nx g @nrwl/workspace:library workspace/meta/tests

npx nx g @nrwl/workspace:library workspace/domain/system
npx nx g @nrwl/workspace:library workspace/domain/core
npx nx g @nrwl/workspace:library workspace/domain/custom
npx nx g @nrwl/workspace:library workspace/domain/generated
npx nx g @nrwl/workspace:library workspace/domain/json/system
npx nx g @nrwl/workspace:library workspace/domain/json/ajax/core
npx nx g @nrwl/workspace:library workspace/domain/tests


npx nx g @nrwl/workspace:library workspace/adapters/memory
npx nx g @nrwl/workspace:library workspace/adapters/tests
