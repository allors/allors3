#! /usr/bin/env node

const path = require('path');
const lnk = require('lnk');

function link(src, dst){
    const basename = path.basename(src);

    lnk([src], dst)
    .then(() => console.log(basename + ' linked') )
    .catch((e) =>  e.errno && e.errno != -4075 ? console.log(e) : console.log('already linked'))
}

// System
link ('../../../System/Workspace/Typescript/libs/workspace/system/src', 'libs/workspace/system');
link ('../../../System/Workspace/Typescript/libs/meta/lazy/system/src', 'libs/meta/lazy/system');
link ('../../../System/Workspace/Typescript/libs/protocol/json/system/src', 'libs/protocol/json/system');
link ('../../../System/Workspace/Typescript/libs/adapters/memory/system/src', 'libs/adapters/memory/system');
