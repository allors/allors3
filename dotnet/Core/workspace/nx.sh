npx create-nx-workspace@latest allors --preset=empty --cli=nx --nx-cloud=false

cd allors

rem npm install -D @nrwl/angular
rem npm install -D @nrwl/react
rem npm install -D @nrwl/gatsby
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






rem npx nx g @nrwl/angular:application angular/app --routing=true --style=scss --e2eTestRunner=none
rem npx nx g @nrwl/angular:application angular/material/app --routing=true --style=scss --e2eTestRunner=none
rem npx nx g @nrwl/gatsby:app gatsby --e2eTestRunner=none

rem npx nx g @nrwl/workspace:library client/fetch
rem npx nx g @nrwl/workspace:library client/tests

rem npx nx g @nrwl/workspace:library data/core

rem npx nx g @nrwl/angular:library angular/core
rem npx nx g @nrwl/angular:library angular/custom
rem npx nx g @nrwl/angular:library angular/services/core

