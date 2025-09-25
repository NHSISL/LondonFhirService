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
import { PatientSearchPage } from './pages/patientSearchPage';
import SearchByNhsNumberPage from './pages/searchByNhsNumberPage';
import ConfirmDetailsPage from './pages/confirmDetailsPage';
import { SendCodePage } from './pages/sendCodePage';
import ConfirmCodePage from './pages/confirmCodePage';
import { OptInOutPage } from './pages/optInOutPage';
import { ThankyouPage } from './pages/thankyouPage';

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
                    path: "nhsNumberSearch",
                    element: <SearchByNhsNumberPage />
                },
                {
                    path: "patientSearch",
                    element: <PatientSearchPage />
                },
                {
                    path: "confirmDetails",
                    element: <ConfirmDetailsPage />
                },
                {
                    path: "confirmCode",
                    element: <ConfirmCodePage />
                },
                {
                    path: "sendCode",
                    element: <SendCodePage />
                },
                {
                    path: "optInOut",
                    element: <OptInOutPage />
                },
                {
                    path: "thankyou",
                    element: <ThankyouPage />
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