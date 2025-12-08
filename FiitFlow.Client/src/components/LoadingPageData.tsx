import { type ReactNode } from "react"

interface LoadingProps {
    isLoading: boolean;
    children: ReactNode;
}

function LoadingPageData({ isLoading, children }: LoadingProps) {
    if (!isLoading) return children;
    else return (
        <div>
            <h1>Подождите</h1>
        </div>
    );
}

export default LoadingPageData