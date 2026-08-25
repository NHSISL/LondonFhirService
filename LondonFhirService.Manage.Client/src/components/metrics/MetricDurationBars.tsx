import type { MetricDurationBarsProps } from "../../models/components/metrics/MetricDurationBarsProps";

const barHeight = "14px";

export function MetricDurationBars({ bars }: MetricDurationBarsProps) {
    if (bars.hasBars === false) {
        return <></>;
    }

    return (
        <div>
            {/*
                Two bars of the same width: the whole request on top, then the same span of time
                broken into the part spent on providers and the part that was not. The figures sit
                alongside, so these carry tooltips rather than repeating the numbers inline.
            */}
            <div
                className="rounded bg-success mb-1"
                style={{ height: barHeight }}
                title={bars.requestTooltip}
                role="img"
                aria-label={bars.requestTooltip} />

            <div className="d-flex rounded overflow-hidden" style={{ height: barHeight }}>
                <div
                    className="bg-primary"
                    style={{ width: `${bars.providerRequestsPercent}%` }}
                    title={bars.providerRequestsTooltip}
                    role="img"
                    aria-label={bars.providerRequestsTooltip} />

                <div
                    className="bg-warning"
                    style={{ width: `${bars.proxyOverheadPercent}%` }}
                    title={bars.proxyOverheadTooltip}
                    role="img"
                    aria-label={bars.proxyOverheadTooltip} />
            </div>
        </div>
    );
}
