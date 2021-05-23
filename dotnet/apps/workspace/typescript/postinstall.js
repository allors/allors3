#! /usr/bin/env node

const path = require('path');
const lnk = require('lnk');

function link(src, dst){
    const basename = path.basename(src);

    lnk([src], dst)
    .then(() => console.log(basename + ' linked') )
    .catch((e) =>  e.errno && e.errno != -4075 ? console.log(e) : console.log('already linked'))
}

// Core
link ('../../../Core/Workspace/Typescript/libs/data/core/src', 'libs/data/core');
link ('../../../Core/Workspace/Typescript/libs/protocol/core/src', 'libs/protocol/core');
link ('../../../Core/Workspace/Typescript/libs/workspace/core/src', 'libs/workspace/core');
link ('../../../Core/Workspace/Typescript/libs/workspace/memory/src', 'libs/workspace/memory');

link ('../../../Core/Workspace/Typescript/libs/meta/core/src', 'libs/meta/core');
link ('../../../Core/Workspace/Typescript/libs/domain/core/src', 'libs/domain/core');

// Base
link ('../../../Base/Workspace/Typescript/libs/meta/base/src', 'libs/meta/base');
link ('../../../Base/Workspace/Typescript/libs/domain/base/src', 'libs/domain/base');
