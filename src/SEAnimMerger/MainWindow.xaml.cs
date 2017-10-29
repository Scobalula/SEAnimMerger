using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using SELib;

namespace SEAnimMerger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private static void MergeAnimations(string[] files)
        {
            // Save new SEAnim
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "SEanim File (*.SEANIM)|*.seanim";
            if (saveFileDialog.ShowDialog() == true)
            {
                // Stopwatch for execution time
                var watch = System.Diagnostics.Stopwatch.StartNew();
                // File list - Sorted
                List<string> anims = files.ToList();
                anims.Sort();
                // Merged Anim
                SEAnim merged = new SEAnim();
                // Absolute Animation
                merged.AnimType = AnimationType.Absolute;
                // Next Start frame for appending next anim
                int next_start_frame = 0;
                // Add each file's data to new SEAnim
                foreach (string file in anims)
                {
                    if (System.IO.Path.GetExtension(file).ToLower() == ".seanim")
                    {
                        SEAnim anim = SEAnim.Read(file);
                        // Copy Bone Modifiers
                        foreach (var BoneModifier in anim.AnimationBoneModifiers)
                            if (!merged.AnimationBoneModifiers.ContainsKey(BoneModifier.Key))
                                merged.AnimationBoneModifiers.Add(BoneModifier.Key, BoneModifier.Value);
                        // Copy Bone Data
                        foreach (string Bone in anim.Bones)
                        {
                            // Add Translation Keys
                            if (anim.AnimationPositionKeys.ContainsKey(Bone))
                                foreach (SEAnimFrame frame in anim.AnimationPositionKeys[Bone])
                                    merged.AddTranslationKey(
                                        Bone,
                                        next_start_frame + frame.Frame,
                                        ((Vector3)frame.Data).X,
                                        ((Vector3)frame.Data).Y,
                                        ((Vector3)frame.Data).Z);
                            // Add Rotation Keys
                            if (anim.AnimationRotationKeys.ContainsKey(Bone))
                                foreach (SEAnimFrame frame in anim.AnimationRotationKeys[Bone])
                                    merged.AddRotationKey(
                                        Bone,
                                        next_start_frame + frame.Frame,
                                        ((Quaternion)frame.Data).X,
                                        ((Quaternion)frame.Data).Y,
                                        ((Quaternion)frame.Data).Z,
                                        ((Quaternion)frame.Data).W);
                            // Add Scale Keys
                            if (anim.AnimationScaleKeys.ContainsKey(Bone))
                                foreach (SEAnimFrame frame in anim.AnimationScaleKeys[Bone])
                                    merged.AddScaleKey(
                                        Bone,
                                        next_start_frame + frame.Frame,
                                        ((Vector3)frame.Data).X,
                                        ((Vector3)frame.Data).Y,
                                        ((Vector3)frame.Data).Z);
                            // Copy Notetracks
                            foreach (KeyValuePair<string, List<SEAnimFrame>> Notetrack in anim.AnimationNotetracks)
                                foreach (SEAnimFrame NoteFrame in Notetrack.Value)
                                    merged.AddNoteTrack(Notetrack.Key, next_start_frame + NoteFrame.Frame);
                        }
                        next_start_frame += anim.FrameCount;
                    }
                }
                // Write new SEAnim
                merged.Write(saveFileDialog.FileName);
                // Stop watch
                watch.Stop();
                float finishtime = watch.ElapsedMilliseconds;
                MessageBox.Show(string.Format("Merged {0} SEAnims in {1} Seconds", anims.Count, finishtime / 1000.000));
            }
        }
        private void DropLabel_Drop(object sender, DragEventArgs dropped_files)
        {
            if (dropped_files.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] dropped = (string[])dropped_files.Data.GetData(DataFormats.FileDrop);
                try
                {
                    MergeAnimations(dropped);
                }
                catch(Exception error)
                {
                    MessageBox.Show(string.Format("Unhandled Exception occured while merging:\n\n{0}", error));
                }
            }

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "SEanim File (*.SEANIM)|*.seanim";
            openFileDialog.Multiselect = true;
            if(openFileDialog.ShowDialog() == true)
            {
                MergeAnimations(openFileDialog.FileNames);
            }

        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("SEAnim Merger 1.0 by Scobalula\n\nSELibDotNet by DTZxPorter", "About");
        }
    }
}
