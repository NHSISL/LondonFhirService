import { ToastContent, toast as rToast } from "react-toastify";

export const toastInfo = (content: ToastContent) => { return rToast.info(content, { toastId: 3 }) };
