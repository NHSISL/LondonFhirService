import { MetricApiBrokerException } from "../../../models/foundations/metrics/exceptions/MetricApiBrokerException";
import { MetricDependencyException } from "../../../models/foundations/metrics/exceptions/MetricDependencyException";
import { MetricServiceException } from "../../../models/foundations/metrics/exceptions/MetricServiceException";
import { MetricValidationException } from "../../../models/foundations/metrics/exceptions/MetricValidationException";

export async function tryCatchMetricServiceAsync<T>(
    returningMetricFunction: () => Promise<T>)
    : Promise<T> {
    try {
        return await returningMetricFunction();
    } catch (exception) {
        if (exception instanceof MetricValidationException) {
            throw exception;
        }

        if (exception instanceof MetricApiBrokerException) {
            throw new MetricDependencyException(
                "Metric dependency error occurred, please contact support.",
                exception);
        }

        throw new MetricServiceException(
            "Metric service error occurred, please contact support.",
            exception);
    }
}
