using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Arc4u.Xamarin.Forms.Mvvm
{
    public class ShellContentPage : ContentPage
    {
        public Dictionary<String, Object> Parameters { get; set; } = new Dictionary<string, Object>();
    }
}
