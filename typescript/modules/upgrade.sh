cd ..

npx create-nx-workspace@latest allors --preset=empty --cli=nx --nx-cloud=false

cd allors

npm install -D jest-chain
npm install -D jest-trx-results-processor
npm install -D @nrwl/angular

npm install @angular/cdk
npm install @angular/material
npm install bootstrap@4.6.0
npm install date-fns
npm install easymde

npx nx g @nrwl/angular:application angular/base/app --routing=true --style=scss --e2eTestRunner=none
npx nx g @nrwl/angular:application angular/core/app --routing=true --style=scss --e2eTestRunner=none

npx nx g @nrwl/workspace:library protocol/json/system

npx nx g @nrwl/workspace:library workspace/adapters/json/system
npx nx g @nrwl/workspace:library workspace/adapters/json/system-tests-async
npx nx g @nrwl/workspace:library workspace/adapters/json/system-tests-reactive
npx nx g @nrwl/workspace:library workspace/adapters/system
npx nx g @nrwl/workspace:library workspace/adapters/system-tests

npx nx g @nrwl/workspace:library workspace/angular/base
npx nx g @nrwl/workspace:library workspace/angular/core

npx nx g @nrwl/workspace:library workspace/configuration/core
npx nx g @nrwl/workspace:library workspace/configuration/base

npx nx g @nrwl/workspace:library workspace/derivations/base
npx nx g @nrwl/workspace:library workspace/derivations/base-custom
npx nx g @nrwl/workspace:library workspace/derivations/core
npx nx g @nrwl/workspace:library workspace/derivations/core-custom

npx nx g @nrwl/workspace:library workspace/domain/base
npx nx g @nrwl/workspace:library workspace/domain/core
npx nx g @nrwl/workspace:library workspace/domain/system

npx nx g @nrwl/workspace:library workspace/meta/base
npx nx g @nrwl/workspace:library workspace/meta/core
npx nx g @nrwl/workspace:library workspace/meta/json/base
npx nx g @nrwl/workspace:library workspace/meta/json/core
npx nx g @nrwl/workspace:library workspace/meta/json/system
npx nx g @nrwl/workspace:library workspace/meta/json/system-tests
npx nx g @nrwl/workspace:library workspace/meta/system
npx nx g @nrwl/workspace:library workspace/meta/system-tests