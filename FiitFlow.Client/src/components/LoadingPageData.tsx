import { type ReactNode } from "react"

interface LoadingProps {
    isLoading: boolean;
    isLoaded?: boolean;
    message?: string;
    children: ReactNode;
}

function LoadingPageData({ isLoading, isLoaded, message, children }: LoadingProps) {
    if (isLoading)
        return <h1>Подождите...</h1>;
    if (isLoaded === undefined || isLoaded)
        return children;
    if (message === undefined)
        return <h1>Загрузка не удалась(</h1>
    return <h1>{message}</h1>;
}

export default LoadingPageData