npx create-nx-workspace@latest allors --preset=empty --cli=nx --nx-cloud=false

cd allors

npm install -D @nrwl/angular
npm install -D @nrwl/react
npm install -D @nrwl/gatsby
npm install -D jest-chain
npm install -D jest-extended

npx nx g @nrwl/angular:application angular/app --e2eTestRunner=none
npx nx g @nrwl/angular:application angular/material/app --e2eTestRunner=none
npx nx g @nrwl/gatsby:app gatsby --e2eTestRunner=none

npx nx g @nrwl/workspace:library client/fetch
npx nx g @nrwl/workspace:library client/tests

npx nx g @nrwl/workspace:library data/core

npx nx g @nrwl/workspace:library domain/core
npx nx g @nrwl/workspace:library domain/custom
npx nx g @nrwl/workspace:library domain/generated

npx nx g @nrwl/workspace:library meta/core
npx nx g @nrwl/workspace:library meta/generated
npx nx g @nrwl/workspace:library meta/tests

npx nx g @nrwl/workspace:library protocol/core

npx nx g @nrwl/workspace:library workspace/core
npx nx g @nrwl/workspace:library workspace/memory
npx nx g @nrwl/workspace:library workspace/tests


