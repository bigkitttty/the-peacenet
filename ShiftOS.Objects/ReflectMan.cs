﻿using System;
        {
            return names.FirstOrDefault(x => x.Name == name) != null;
        }
            {
                {
                    ret.AddRange(asm.GetTypes());
                }
                catch (ReflectionTypeLoadException rtlex)
                {
                    bool suppressLoaderErrors = UserConfig.Get().SuppressTypeLoadErrors;
                    bool cont = suppressLoaderErrors;
                    if (!suppressLoaderErrors)
                    {
                        StringBuilder messageBuilder = new StringBuilder();
                        messageBuilder.AppendLine($"An error has occured while loading the following assembly: {asm.FullName}");
                        messageBuilder.AppendLine();
                        messageBuilder.AppendLine(rtlex.Message);
                        messageBuilder.AppendLine();
                        messageBuilder.AppendLine("The following internal errors occurred:");
                        messageBuilder.AppendLine();
                        foreach(var cex in rtlex.LoaderExceptions)
                        {
                            if(cex != null)
                            {
                                messageBuilder.AppendLine(cex.GetType().Name + ": " + cex.Message);
                                messageBuilder.AppendLine();
                            }
                        }
                        messageBuilder.AppendLine($"Would you like to attempt to continue loading modules? Clicking 'No' will terminate the process while clicking 'Yes' will ignore the failing types and continue to load.");
                        var result = System.Windows.Forms.MessageBox.Show(
                            caption: "ReflectMan - Error loading types",
                            text: messageBuilder.ToString(),
                            buttons: System.Windows.Forms.MessageBoxButtons.YesNo,
                            icon: System.Windows.Forms.MessageBoxIcon.Error
                            );
                        cont = result == System.Windows.Forms.DialogResult.Yes;
                    }
                    if(cont == false)
                    {
                        Environment.Exit(0);
                    }
                    else
                    {
                        ret.AddRange(rtlex.Types.Where(x => x != null));
                    }
                }
            }
            types = ret.ToArray();