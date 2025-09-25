import { ToastContent, toast as rToast } from "react-toastify";

export const toastError = (content: ToastContent) => { return rToast.error(content, { toastId: 2 }) };
