cd ..

npx create-nx-workspace@latest allors --preset=empty --cli=nx --nx-cloud=false

cd allors

npm install -D jest-chain
npm install -D jest-trx-results-processor
npm install -D @nrwl/angular

npm install @angular/cdk
npm install @angular/material
npm install @angular/material-luxon-adapter
npm install bootstrap@4.6.0
npm install common-tags
npm install cross-fetch
npm install date-fns
npm install easymde
npm install jsnlog
npm install luxon

// Apps Extranet
npx nx g @nrwl/angular:application angular-material/extranet/apps --routing=true --style=scss --e2eTestRunner=none
npx nx g @nrwl/workspace:library workspace/angular-material/extranet/apps
npx nx g @nrwl/workspace:library workspace/derivations/extranet/apps
npx nx g @nrwl/workspace:library workspace/derivations/extranet/apps-custom
npx nx g @nrwl/workspace:library workspace/domain/extranet/apps
npx nx g @nrwl/workspace:library workspace/meta/extranet/apps
npx nx g @nrwl/workspace:library workspace/meta/json/extranet/apps

// Apps Intranet
npx nx g @nrwl/angular:application angular-material/intranet/apps --routing=true --style=scss --e2eTestRunner=none
npx nx g @nrwl/workspace:library workspace/angular-material/intranet/apps
npx nx g @nrwl/workspace:library workspace/derivations/intranet/apps
npx nx g @nrwl/workspace:library workspace/derivations/intranet/apps-custom
npx nx g @nrwl/workspace:library workspace/domain/intranet/apps
npx nx g @nrwl/workspace:library workspace/meta/intranet/apps
npx nx g @nrwl/workspace:library workspace/meta/json/intranet/apps

// Base
npx nx g @nrwl/angular:application angular/foundation/base --routing=true --e2eTestRunner=none
npx nx g @nrwl/angular:application angular/application/base --routing=true --e2eTestRunner=none
npx nx g @nrwl/angular:application angular-material/foundation/base --routing=true --style=scss --e2eTestRunner=none
npx nx g @nrwl/angular:application angular-material/application/base --routing=true --style=scss --e2eTestRunner=none
npx nx g @nrwl/workspace:library workspace/angular/foundation/base
npx nx g @nrwl/workspace:library workspace/angular/application/base
npx nx g @nrwl/workspace:library workspace/angular-material/foundation/base
npx nx g @nrwl/workspace:library workspace/angular-material/application/base
npx nx g @nrwl/workspace:library workspace/derivations/base
npx nx g @nrwl/workspace:library workspace/derivations/base-custom
npx nx g @nrwl/workspace:library workspace/domain/base
npx nx g @nrwl/workspace:library workspace/meta/base
npx nx g @nrwl/workspace:library workspace/meta/json/base

// Core
npx nx g @nrwl/workspace:library workspace/derivations/core
npx nx g @nrwl/workspace:library workspace/derivations/core-custom
npx nx g @nrwl/workspace:library workspace/domain/core
npx nx g @nrwl/workspace:library workspace/meta/core
npx nx g @nrwl/workspace:library workspace/meta/json/core

// System
npx nx g @nrwl/workspace:library protocol/json/system

npx nx g @nrwl/workspace:library workspace/adapters/json/system
npx nx g @nrwl/workspace:library workspace/adapters/json/system-tests
npx nx g @nrwl/workspace:library workspace/adapters/system
npx nx g @nrwl/workspace:library workspace/adapters/system-tests

npx nx g @nrwl/workspace:library workspace/derivations/system
npx nx g @nrwl/workspace:library workspace/domain/system

npx nx g @nrwl/workspace:library workspace/meta/json/system
npx nx g @nrwl/workspace:library workspace/meta/json/system-tests
npx nx g @nrwl/workspace:library workspace/meta/system
npx nx g @nrwl/workspace:library workspace/meta/system-tests