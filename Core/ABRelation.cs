using System.Collections.Generic;

namespace ABExplorer.Core
{
    public class AbRelation
    {
        private readonly List<string> _dependenceAbList;
        private readonly List<string> _referenceAbList;

        public AbRelation()
        {
            _dependenceAbList = new List<string>();
            _referenceAbList = new List<string>();
        }

        public void AddDependence(string abName)
        {
            if (!_dependenceAbList.Contains(abName))
            {
                _dependenceAbList.Add(abName);
            }
        }

        public bool RemoveDependence(string abName)
        {
            if (_dependenceAbList.Contains(abName))
            {
                _dependenceAbList.Remove(abName);
            }

            return !(_dependenceAbList.Count > 0);
        }

        public List<string> GetAllDependence()
        {
            return _dependenceAbList;
        }

        public void AddReference(string abName)
        {
            if (!_referenceAbList.Contains(abName))
            {
                _referenceAbList.Add(abName);
            }
        }

        public bool RemoveReference(string abName)
        {
            if (_referenceAbList.Contains(abName))
            {
                _referenceAbList.Remove(abName);
            }

            return !(_referenceAbList.Count > 0);
        }

        public List<string> GetAllReference()
        {
            return _referenceAbList;
        }
    }
}