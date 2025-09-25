import { useQuery } from '@tanstack/react-query';
import FeatureBroker from "../../brokers/apiBroker.features";
import { Feature } from "../../models/features/feature";

export const featureService = {
    useGetAllFeatures: () => {
        const featureBroker = new FeatureBroker();

        return useQuery<Feature[]>({
            queryKey: ["FeaturesGetAll"],
            queryFn: async () => await featureBroker.GetAllFeatureAsync(),
            staleTime: Infinity
        });
    }
};