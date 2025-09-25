import { ToastContent, toast as rToast } from "react-toastify";

export const toastWarning = (content: ToastContent) => { return rToast.warn(content, { toastId: 1 }) };