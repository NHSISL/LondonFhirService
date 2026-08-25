// One span of a correlation, as the detail table renders it. depth drives the indent that shows
// the tree without needing a nested component.
export type MetricSpanView = {
    id: string;
    depth: number;
    typeText: string;
    nameText: string;
    targetText: string;
    statusText: string;
    statusClassName: string;
    startedText: string;
    durationText: string;
    payloadText: string;
    errorCodeText: string;
    descriptionText: string;

    // Timeline geometry, as percentages of the whole correlation window so the component can
    // place a bar without knowing anything about clocks or durations.
    offsetPercent: number;
    widthPercent: number;
    offsetText: string;
    barClassName: string;

    // What the timeline puts in front of the bar. Provider spans carry the provider name, because
    // a fan out draws one identical "Provider" row per provider otherwise.
    labelText: string;
};
