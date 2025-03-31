

namespace GitControl
{    public enum GitFeatureType
    {
        Repositories,
        Branches,
        Overview
    };
    
    public class GitFeature
    {
        
        public string FeatureName { get; set; }
        public GitFeatureType FeatureType { get; set; }

        public GitFeature(string featureName, GitFeatureType featureType)
        {
            FeatureName = featureName;
            FeatureType = featureType;
        }

    }
}
