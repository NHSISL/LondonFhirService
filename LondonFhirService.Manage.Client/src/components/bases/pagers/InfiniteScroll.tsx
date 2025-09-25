import React, { FunctionComponent } from "react";
import useInfiniteScroll from "react-infinite-scroll-hook";

interface InfiniteScrollProps {
    children?: React.ReactNode;
    loading: boolean;
    hasNextPage: boolean;
    loadMore: VoidFunction
}

const InfiniteScroll: FunctionComponent<InfiniteScrollProps> = (props) => {

    const [scrollRef] = useInfiniteScroll({
        loading: props.loading,
        hasNextPage: props.hasNextPage,
        onLoadMore: props.loadMore
    })

    return (
        <>
            {props.children}
            <span ref={scrollRef} />
        </>
    )
}

export default InfiniteScroll
