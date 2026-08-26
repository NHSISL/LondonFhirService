/* eslint-disable @typescript-eslint/no-explicit-any */
import { createBrowserRouter, Navigate, RouterProvider } from 'react-router-dom';
import './App.css';
import Root from './components/root';
import ErrorPage from './errors/error';
import { MsalProvider } from '@azure/msal-react';
import { QueryClientProvider } from '@tanstack/react-query';
import { queryClientGlobalOptions } from './brokers/apiBroker.globals';
import { Home } from './pages/home';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import "react-toastify/dist/ReactToastify.css";
import ToastBroker from './brokers/toastBroker';
import { TestPage } from './pages/testPage';
import { ProvidersPage } from './pages/providers/ProvidersPage';
import { ProviderDetailPage } from './pages/providers/ProviderDetailPage';
import { ProviderAddPage } from './pages/providers/ProviderAddPage';
import { ComparisonsPage } from './pages/comparisons/ComparisonsPage';
import { ComparisonDetailPage } from './pages/comparisons/ComparisonDetailPage';
import { AuditsPage } from './pages/audits/AuditsPage';
import { AuditDetailPage } from './pages/audits/AuditDetailPage';
import { MetricsPage } from './pages/metrics/MetricsPage';
import { MetricDetailPage } from './pages/metrics/MetricDetailPage';
import { SecuredRoute } from './components/securitys/securedRoutes';
import securityPoints from './securityMatrix';

function App({ instance }: any) {

    const router = createBrowserRouter([
        {
            path: "/",
            element: <Root />,
            errorElement: <ErrorPage />,
            children: [
                {
                    path: "home",
                    element: <Home />
                },
                {
                    path: "admin/audits",
                    element: (
                        <SecuredRoute allowedRoles={securityPoints.audits.view}>
                            <AuditsPage />
                        </SecuredRoute>
                    )
                },
                {
                    path: "admin/audits/:auditId",
                    element: (
                        <SecuredRoute allowedRoles={securityPoints.audits.view}>
                            <AuditDetailPage />
                        </SecuredRoute>
                    )
                },
                {
                    path: "admin/metrics",
                    element: (
                        <SecuredRoute allowedRoles={securityPoints.metrics.view}>
                            <MetricsPage />
                        </SecuredRoute>
                    )
                },
                {
                    path: "admin/metrics/:correlationId",
                    element: (
                        <SecuredRoute allowedRoles={securityPoints.metrics.view}>
                            <MetricDetailPage />
                        </SecuredRoute>
                    )
                },
                {
                    path: "admin/comparisons",
                    element: (
                        <SecuredRoute allowedRoles={securityPoints.comparisons.view}>
                            <ComparisonsPage />
                        </SecuredRoute>
                    )
                },
                {
                    path: "admin/comparisons/:fhirRecordDifferenceId",
                    element: (
                        <SecuredRoute allowedRoles={securityPoints.comparisons.view}>
                            <ComparisonDetailPage />
                        </SecuredRoute>
                    )
                },
                {
                    path: "admin/providers",
                    element: (
                        <SecuredRoute allowedRoles={securityPoints.providers.view}>
                            <ProvidersPage />
                        </SecuredRoute>
                    )
                },
                {
                    path: "admin/providers/new",
                    element: (
                        <SecuredRoute allowedRoles={securityPoints.providers.add}>
                            <ProviderAddPage />
                        </SecuredRoute>
                    )
                },
                {
                    path: "admin/providers/:providerId",
                    element: (
                        <SecuredRoute allowedRoles={securityPoints.providers.view}>
                            <ProviderDetailPage />
                        </SecuredRoute>
                    )
                },
                {
                    path: "testPage",
                    element: <TestPage />
                },
                {
                    index: true,
                    element: <Navigate to="/home" />
                },
            ]
        }
    ]);

    return (
        <>
            <MsalProvider instance={instance}>
                <QueryClientProvider client={queryClientGlobalOptions}>
                    <RouterProvider router={router} />
                    <ReactQueryDevtools initialIsOpen={false} />
                </QueryClientProvider>
                <ToastBroker.Container />
            </MsalProvider>
        </>
    );


}

export default App;