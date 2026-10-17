export function initialiseWorkspaceExamples() {
  for (const root of document.querySelectorAll<HTMLElement>('[data-workspace-example]')) {
    const scenario = root.querySelector<HTMLSelectElement>('[data-scenario-select]')!;
    const step = root.querySelector<HTMLSelectElement>('[data-step-select]')!;
    const previous = root.querySelector<HTMLButtonElement>('[data-previous]')!;
    const next = root.querySelector<HTMLButtonElement>('[data-next]')!;
    const live = root.querySelector<HTMLElement>('[data-live-frame]')!;
    const summary = root.querySelector<HTMLElement>('[data-scenario-summary]')!;
    const title = root.querySelector<HTMLElement>('[data-scenario-title]')!;
    const status = root.querySelector<HTMLElement>('[data-step-status]')!;
    const transcript = root.querySelector<HTMLElement>('[data-scenario-transcript]')!;
    let frames: HTMLElement[] = [];

    function showStep() {
      const index = step.selectedIndex;
      const frame = frames[index]!;
      live.replaceChildren(frame.cloneNode(true));
      previous.disabled = index === 0;
      next.disabled = index === frames.length - 1;
      const outcome = frame.querySelector('[data-outcome]')!.textContent;
      status.textContent = `${scenario.selectedOptions[0]!.textContent}. Step ${index + 1} of ${frames.length}: ${frame.dataset.stepTitle}. ${outcome}.`;
    }

    function chooseScenario() {
      const source = Array.from(transcript.querySelectorAll<HTMLElement>('[data-scenario-source]'))
        .find((element) => element.dataset.scenarioSource === scenario.value)!;
      summary.textContent = source.querySelector('[data-summary]')!.textContent;
      title.textContent = scenario.selectedOptions[0]!.textContent;
      frames = Array.from(source.querySelectorAll<HTMLElement>('[data-frame]'));
      step.replaceChildren(...frames.map((frame, index) =>
        new Option(`${index + 1}. ${frame.dataset.stepTitle}`, frame.dataset.stepId)));
      showStep();
    }

    scenario.addEventListener('change', chooseScenario);
    step.addEventListener('change', showStep);
    previous.addEventListener('click', () => { step.selectedIndex--; showStep(); });
    next.addEventListener('click', () => { step.selectedIndex++; showStep(); });
    chooseScenario();
    root.querySelector<HTMLElement>('[data-controls]')!.hidden = false;
  }
}
