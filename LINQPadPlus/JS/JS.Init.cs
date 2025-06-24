using LINQPad;
using LINQPadPlus._sys;

namespace LINQPadPlus;

public static partial class JS
{
	internal static void Init()
	{
		Util.InvokeScript(false, "eval", Fmt(
			"""

			// ***********
			// * Logging *
			// ***********
			function log(str) { console.log(str); external.log(str); }
			function logError(err, code, srcMember, srcFile, srcLine) {
				log(' ');
				log(`JavaScript Exception: ${err.message}`);
				log('====================');
				log(' ');
				log(`CallerMemberName: ${srcMember}`);
				log(`CallerFilePath  : ${srcFile}`);
				log(`CallerLineNumber: ${srcLine}`);
				log(' ');
				log('Stack');
				log('-----');
				log(err.stack);
				log(' ');
				log('Code');
				log('----');
				log(code);
			}
			
			function dispatchError(err, runId) {
				dispatch(____0____, {
					id: ____1____,
					run_id: runId,
					message: err.message,
					stack: err.stack,
				});
			}



			// ******************
			// * waitForElement *
			// ******************
			function waitForElement(eltId)
			{
				const selectorId = `#${eltId}`;
				return new Promise(resolve => {
					const element = document.querySelector(selectorId);
					if (element) {
						resolve(element);
					}
					const observer = new MutationObserver(() => {
						const element = document.querySelector(selectorId);
						if (element) {
							observer.disconnect();
							resolve(element);
						}
						observer.takeRecords();
					});
					const target = document.body;
					observer.observe(target, {
						childList: true,
						subtree: true,
						attributes: true
					});
				});
			}



			// **********************
			// * getElementsByXPath *
			// **********************
			function getElementsByXPath(xpath)
			{
			    const query = document.evaluate(xpath, document, null, XPathResult.ORDERED_NODE_SNAPSHOT_TYPE, null);
			    const results = [];
			    for (let i = 0, length = query.snapshotLength; i < length; ++i) results.push(query.snapshotItem(i));
			    return results;
			}



			// *****************
			// * setIntervalEx *
			// *****************
			if (!timerIds) {
				var timerIds = [];
			} else {
				for (var timerId of timerIds) clearInterval(timerId);
			}
			function setIntervalEx(callback, delay) {
				var intervalId = setInterval(() => { callback(stop); }, delay);
				function stop() { clearInterval(intervalId); }
				timerIds.push(intervalId);
			}
			
			
			
			
			// ************************************************
			// * Remove all elements with class=ClassToRemove *
			// ************************************************
			new MutationObserver((muts, obs) => {
				muts.filter(e => e.type === 'childList')
					.flatMap(e => [...e.addedNodes])
					.filter(e => e instanceof HTMLElement && (e.classList.contains(____2____)|| [...e.children].some(f => f.classList.contains(____2____))))
					.forEach(del => {
						console.log('EVT');
						del.remove();
					});
			}).observe(document.body, { childList: true, subtree: true });
			
			

			""",
			e => e
				.JSRepl_Val(0, Events.ErrorDispatcherId)
				.JSRepl_Val(1, JSRunIdentifiers.RuntimeErrorIdentifier)
				.JSRepl_Val(2, JSRunIdentifiers.ClassToRemove)
		));
	}
}