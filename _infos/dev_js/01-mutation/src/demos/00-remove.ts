import { btn } from "./_common";

const eltId = 'myid';
const removeCls = 'del-cls';

btn('Add', () => {
    const elt = document.createElement('span');
    elt.id = eltId;
    elt.textContent = "My span";
    document.body.appendChild(elt);
})

btn('Add(remove)', () => {
    const elt = document.createElement('span');
    elt.id = eltId;
    elt.classList.add(removeCls);
    elt.textContent = "My span (remove)";

    const eltWrap = document.createElement('div');
    eltWrap.appendChild(elt);

    document.body.appendChild(eltWrap);
})

btn('Del', () => {
    const elt = document.getElementById(eltId);
    elt?.remove();
})

btn('DelAll', () => {
    while (true) {
        const elt = document.getElementById(eltId);
        elt?.remove();
        if (!elt) break;
    }
})


new MutationObserver((muts, obs) => {
	muts.filter(e => e.type === 'childList')
		.flatMap(e => [...e.addedNodes])
		.filter(e => e instanceof HTMLElement && (e.classList.contains(removeCls) || [...e.children].some(f => f.classList.contains(removeCls))))
		.forEach(e => {
            console.log('Evt');
            e.remove();
            //console.log(`  isHTMLElement: ${e instanceof HTMLElement}`);
            //console.log(`  isCls: ${e.classList.contains('ClassToRemove')}`);
            //console.log(`  kids: ${[...e.children].length}`);
            //if ([...e.children].length === 1) {
            //    const kid = [...e.children][0];
            //    console.log(kid);
            //    console.log(kid.classList.contains('del-cls'));
            //}
            //console.log(`  isKidCls: ${[...e.children].some(f => f.classList.contains('ClassToRemove'))}`);
			//console.log(e);
			//del.remove();
		});
}).observe(document.body, { childList: true, subtree: true });


/*new MutationObserver((muts, obs) => {
    muts = muts.filter(e => e.type === 'childList');
    const mutsAdd = muts.flatMap(e => [...e.addedNodes]).filter(e => isElt(e) && e.id === eltId);
    const mutsDel = muts.flatMap(e => [...e.removedNodes]).filter(e => isElt(e) && e.id === eltId);
    if (mutsAdd.length > 0) {
        console.log('Add');
        const toDels = mutsAdd.filter(e => isElt(e) && e.classList.contains(removeCls));
        for (const toDel of toDels)
            toDel.remove();
    }
    if (mutsDel.length > 0) {
        console.log('Del');
    }
}).observe(document.body, { childList: true, subtree: true });*/



function isElt(node: Node): node is HTMLElement { return node instanceof HTMLElement; }