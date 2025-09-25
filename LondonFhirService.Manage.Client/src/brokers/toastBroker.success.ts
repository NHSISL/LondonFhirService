import { ToastContent, toast as rToast } from "react-toastify";

export const toastSuccess = (content: ToastContent) => { return rToast.success(content, { toastId: 4 }) };
