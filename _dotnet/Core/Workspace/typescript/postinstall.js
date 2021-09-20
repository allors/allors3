#! /usr/bin/env node

const path = require('path');
const lnk = require('lnk');

function link(src, dst){
    const basename = path.basename(src);

    lnk([src], dst)
    .then(() => console.log(basename + ' linked') )
    .catch((e) =>  e.errno && e.errno != -4075 ? console.log(e) : console.log('already linked'))
}

link ('../../../System/Workspace/Typescript/libs/shared/system/src', 'libs/shared/system');
link ('../../../System/Workspace/Typescript/libs/protocol/json/system/src', 'libs/protocol/json/system');

link ('../../../System/Workspace/Typescript/libs/workspace/meta/system/src', 'libs/workspace/meta/system');
link ('../../../System/Workspace/Typescript/libs/workspace/meta/lazy/system/src', 'libs/workspace/meta/lazy/system');

link ('../../../System/Workspace/Typescript/libs/workspace/domain/system/src', 'libs/workspace/domain/system');
link ('../../../System/Workspace/Typescript/libs/workspace/domain/json/system/src', 'libs/workspace/domain/json/system');
