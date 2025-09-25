import { MutationCache, QueryCache, QueryClient } from '@tanstack/react-query';

export const queryClientGlobalOptions = new QueryClient({
    defaultOptions: {
        queries: {
            retry: false
        }
    },
    queryCache: new QueryCache({
        onError: (error: Error) => {
            //toastError("An unknown error has occurred, please refresh the page and try again.");
            console.log("An unknown error has occurred, please refresh the page and try again.");

            throw error;
        }
    }),
    mutationCache: new MutationCache({
        onSuccess: () => {
           // toastSuccess("Saved.");
            console.log("Saved.");
        },
        onError: (error: Error) => {
            if (error) {
                console.log("An unknown error has occurred, please try again.");
                //toastError("An unknown error has occurred, please try again.");
            } else {
                //toastWarning("Your record has not been saved, please correct and try again.");
                console.log("Your record has not been saved, please correct and try again.")
                // throw new ApiValidationError(error?.response?.data?.errors);
            }

            throw error;
        }
    })
});