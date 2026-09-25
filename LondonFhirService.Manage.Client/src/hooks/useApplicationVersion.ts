import { useMemo } from "react";
import { useQuery } from "@tanstack/react-query";
import { VersionViewService } from "../services/views/versions/versionViewService";

// The release for the header. Fetched once and kept for the life of the page: it only changes on
// a deployment, and a deployment reloads the page. A failure shows nothing rather than an error -
// the header is not the place to report that a version could not be read.
export function useApplicationVersion(): { versionText: string | null } {
    const versionViewService = useMemo(() => new VersionViewService(), []);

    const { data } = useQuery({
        queryKey: ["ApplicationVersionView"],
        queryFn: async ({ signal }) => await versionViewService.retrieveVersionViewAsync(signal),
        staleTime: Infinity,
        retry: false
    });

    return { versionText: data?.versionText ?? null };
}
