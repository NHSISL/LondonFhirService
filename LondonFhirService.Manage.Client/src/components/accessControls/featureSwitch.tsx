import React, { FunctionComponent } from 'react';
import { FeatureDefinitions } from '../../featureDefinitions';
import { featureService } from '../../services/foundations/featureService';

type FeatureSwitchProps = {
    feature: FeatureDefinitions;
    children: React.ReactNode;
}

export const FeatureSwitch: FunctionComponent<FeatureSwitchProps> = (props) => {
    const { feature, children } = props;
    const { data, isLoading } = featureService.useGetAllFeatures();

    if (isLoading || !data) {
        return <></>
    }

    if (data.findIndex(f => f.feature === feature) > -1) {
        return <>{children}</>;
    }

    return <></>;
}
