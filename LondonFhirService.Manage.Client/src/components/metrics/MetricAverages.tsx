import { Table } from "react-bootstrap";
import { MetricDurationBars } from "./MetricDurationBars";
import type { MetricAveragesProps } from "../../models/components/metrics/MetricAveragesProps";

export function MetricAverages({ averages }: MetricAveragesProps) {
    return (
        <div className="d-inline-block border rounded">
            <Table size="sm" borderless className="mb-0">
                <caption className="visually-hidden">
                    Average request timings. {averages.sampleText}.
                </caption>

                <tbody>
                    <tr>
                        <th scope="row" className="fw-normal pe-4">Avg request time</th>
                        <td className="text-end">{averages.averageRequestText}</td>
                    </tr>
                    <tr>
                        <th scope="row" className="fw-normal pe-4">Avg provider requests</th>
                        <td className="text-end">{averages.averageProviderRequestsText}</td>
                    </tr>
                    <tr>
                        <th scope="row" className="fw-normal pe-4">Avg proxy overhead</th>
                        <td className="text-end">{averages.averageProxyOverheadText}</td>
                    </tr>
                </tbody>
            </Table>

            <div className="px-2 pb-2" style={{ minWidth: "220px" }}>
                <MetricDurationBars bars={averages.bars} />
            </div>

            <p className="text-muted small mb-0 px-2 pb-1">{averages.sampleText}</p>
        </div>
    );
}
