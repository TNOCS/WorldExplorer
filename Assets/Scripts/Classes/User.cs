using System;
using UnityEngine;

namespace Assets.Scripts.Classes
{
    // A delegate type for hooking up change notifications.
    public delegate void SelectedFeatureChangedEventHandler(object sender, EventArgs e);

    public class User
    {
        private Guid id;
        private Color selectionColor;
        private string selectedFeatureId;

        public User(Guid id)
        {
            this.id = id;
        }

        public User(Guid id, Color selectionColor) : this(id)
        {
            this.selectionColor = selectionColor;
        }

        public Guid Id { get { return id; } }

        public Color SelectionColor
        {
            get { return selectionColor; }
            set { selectionColor = value; }
        }

        public string SelectedFeatureId
        {
            get { return selectedFeatureId; }
            set { selectedFeatureId = value; }
        }
    }
}