﻿using STROOP.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STROOP.Structs
{
    public class ObjectBehaviorAssociation
    {
        public BehaviorCriteria BehaviorCriteria;

        public string Name;
        public bool RotatesOnMap;
        public string ImagePath = "";
        public string MapImagePath = "";
        public Image Image;
        public Image TransparentImage;
        public Image MapImage;
        public List<WatchVariableControlPrecursor> Precursors = new List<WatchVariableControlPrecursor>();

        private List<WatchVariableControl> _watchVariableControls = null;
        public List<WatchVariableControl> WatchVariableControls
        {
            get
            {
                if (_watchVariableControls == null)
                {
                    _watchVariableControls =
                        Precursors.ConvertAll(
                            precursor => precursor.CreateWatchVariableControl());
                }
                return _watchVariableControls;
            }
        }

        public bool MeetsCriteria(BehaviorCriteria behaviorCriteria)
        {
            return BehaviorCriteria.CongruentTo(behaviorCriteria);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ObjectBehaviorAssociation))
                return false;

            var otherBehavior = (ObjectBehaviorAssociation)obj;
            return otherBehavior == this;
        }

        public override int GetHashCode()
        {
            return BehaviorCriteria.GetHashCode();
        }

        public static bool operator ==(ObjectBehaviorAssociation a, ObjectBehaviorAssociation b)
        {
            if (object.ReferenceEquals(a, null))
            {
                return object.ReferenceEquals(b, null);
            }
            else if (object.ReferenceEquals(b, null))
                return false;

            return a.BehaviorCriteria == b.BehaviorCriteria;
        }

        public static bool operator !=(ObjectBehaviorAssociation a, ObjectBehaviorAssociation b)
        {
            return !(a == b);
        }
    }
}
