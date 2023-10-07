/*

  Map TO JSON is copyright ©2023 Solra Bizna.

  Map To JSON is free software: you can redistribute it and/or modify it under
  the terms of the GNU General Public License as published by the Free Software
  Foundation, either version 2 of the License, or (at your option) any later
  version.

  Map To JSON is distributed in the hope that it will be useful, but WITHOUT
  ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
  FOR A PARTICULAR PURPOSE. See the GNU General Public License for more
  details.

  You should have received a copy of the GNU General Public License along with
  this program. If not, see https://www.gnu.org/licenses/.

*/

using Gtk;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Reflection;
using Weland;

[assembly:AssemblyVersionAttribute("0.0.0.0")]
[assembly:AssemblyTitleAttribute("Map To JSON")]
[assembly:AssemblyDescriptionAttribute("Export all map data into JSON")]
[assembly:AssemblyCopyrightAttribute("©2023 Solra Bizna (GPL-2 or later)")]
public class Plugin {
  private static string lastExportFolder = null;
  public static bool Compatible() {
    return true; // :)
  }
  public static string Name() {
    return "Map To JSON";
  }
  private static JObject JsonifyLightFunction(Light.Function func) {
    JObject ret = new JObject();
    ret.Add("function", func.LightingFunction.ToString());
    ret.Add("period", func.Period);
    ret.Add("deltaPeriod", func.DeltaPeriod);
    ret.Add("intensity", func.Intensity);
    ret.Add("deltaIntensity", func.DeltaIntensity);
    return ret;
  }
  private static JObject JsonifyLight(Light light) {
    JObject ret = new JObject();
    ret.Add("type", light.Type.ToString());
    ret.Add("initiallyActive", light.InitiallyActive);
    // I don't actually know what this flag means. Unused? :|
    //ret.Add("slavedIntensities", light.SlavedIntensities);
    ret.Add("stateless", light.Stateless);
    ret.Add("phase", light.Phase);
    if(light.TagIndex >= 0)
      ret.Add("tag", light.TagIndex);
    else
      ret.Add("tag", null);
    ret.Add("primaryActive", JsonifyLightFunction(light.PrimaryActive));
    ret.Add("secondaryActive", JsonifyLightFunction(light.SecondaryActive));
    ret.Add("becomingActive", JsonifyLightFunction(light.BecomingActive));
    ret.Add("primaryInactive", JsonifyLightFunction(light.PrimaryInactive));
    ret.Add("secondaryInactive", JsonifyLightFunction(light.SecondaryInactive));
    ret.Add("becomingInactive", JsonifyLightFunction(light.BecomingInactive));
    return ret;
  }
  private static JObject JsonifyPoint(Point point) {
    JObject ret = new JObject();
    ret.Add("x", point.X);
    ret.Add("y", point.Y);
    return ret;
  }
  private static JObject JsonifyLine(Line line) {
    JObject ret = new JObject();
    JArray points = new JArray();
    points.Add(line.EndpointIndexes[0]);
    points.Add(line.EndpointIndexes[1]);
    ret.Add("points", points);
    //ret.Add("length", line.Length);
    ret.Add("decorative", (line.Flags & LineFlags.Decorative) != 0);
    ret.Add("hasTransparentSide", (line.Flags & LineFlags.HasTransparentSide) != 0);
    ret.Add("variableElevation", (line.Flags & LineFlags.VariableElevation) != 0);
    ret.Add("elevation", (line.Flags & LineFlags.Elevation) != 0);
    ret.Add("landscape", (line.Flags & LineFlags.Landscape) != 0);
    ret.Add("transparent", (line.Flags & LineFlags.Transparent) != 0);
    ret.Add("solid", (line.Flags & LineFlags.Solid) != 0);
    ret.Add("highestAdjacentFloor", line.HighestAdjacentFloor);
    ret.Add("lowestAdjacentCeiling", line.LowestAdjacentCeiling);
    if(line.ClockwisePolygonSideIndex >= 0)
      ret.Add("clockwiseSideIndex", line.ClockwisePolygonSideIndex);
    else
      ret.Add("clockwiseSideIndex", null);
    if(line.CounterclockwisePolygonSideIndex >= 0)
      ret.Add("counterclockwiseSideIndex", line.CounterclockwisePolygonSideIndex);
    else
      ret.Add("counterclockwiseSideIndex", null);
    if(line.ClockwisePolygonOwner >= 0)
      ret.Add("clockwisePolygonOwner", line.ClockwisePolygonOwner);
    else
      ret.Add("clockwisePolygonOwner", null);
    if(line.CounterclockwisePolygonOwner >= 0)
      ret.Add("counterclockwisePolygonOwner", line.CounterclockwisePolygonOwner);
    else
      ret.Add("counterclockwisePolygonOwner", null);
    return ret;
  }
  private static JObject JsonifyShapeDescriptor(ShapeDescriptor sd) {
    if(sd.IsEmpty()) return null;
    else {
      JObject ret = new JObject();
      ret.Add("collection", sd.Collection);
      ret.Add("clut", sd.CLUT);
      ret.Add("bitmap", sd.Bitmap);
      return ret;
    }
  }
  private static JObject JsonifyPolygon(Polygon poly) {
    JObject ret = new JObject();
    ret.Add("type", poly.Type.ToString());
    // Not used?
    //ret.Add("flags", poly.Flags);
    ret.Add("permutation", poly.Permutation.ToString());
    JArray verts = new JArray();
    JArray lines = new JArray();
    JArray sides = new JArray();
    JArray adjacentPolys = new JArray();
    for(int i = 0; i < poly.VertexCount; ++i) {
      verts.Add(poly.EndpointIndexes[i]);
      lines.Add(poly.LineIndexes[i]);
      sides.Add(poly.SideIndexes[i]);
      if(poly.AdjacentPolygonIndexes[i] >= 0) {
        adjacentPolys.Add(poly.AdjacentPolygonIndexes[i]);
      } else {
        adjacentPolys.Add(null);
      }
    }
    ret.Add("vertices", verts);
    ret.Add("lines", lines);
    ret.Add("sides", sides);
    ret.Add("adjacentPolygons", adjacentPolys);
    ret.Add("floorTexture", JsonifyShapeDescriptor(poly.FloorTexture));
    ret.Add("ceilingTexture", JsonifyShapeDescriptor(poly.CeilingTexture));
    ret.Add("floorHeight", poly.FloorHeight);
    ret.Add("ceilingHeight", poly.CeilingHeight);
    ret.Add("floorLight", poly.FloorLight);
    ret.Add("ceilingLight", poly.CeilingLight);
    // This information isn't necessary to reconstruct the map
    /*
    if(poly.FirstObjectIndex >= 0) {
      ret.Add("firstObjectIndex", poly.FirstObjectIndex);
    }
    */
    ret.Add("floorTransferMode", poly.FloorTransferMode);
    ret.Add("ceilingTransferMode", poly.CeilingTransferMode);
    if(poly.MediaIndex >= 0) {
      JObject media = new JObject();
      media.Add("index", poly.MediaIndex);
      media.Add("light", poly.MediaLight);
      ret.Add("media", media);
    } else {
      ret.Add("media", null);
    }
    if(poly.AmbientSound >= 0)
      ret.Add("ambientSound", poly.AmbientSound);
    else
      ret.Add("ambientSound", null);
    if(poly.RandomSound >= 0)
      ret.Add("randomSound", poly.RandomSound);
    else
      ret.Add("randomSound", null);
    return ret;
  }
  private static JObject JsonifyObject(MapObject obj) {
    JObject ret = new JObject();
    ret.Add("type", obj.Type.ToString());
    ret.Add("subtype", obj.Index);
    if(obj.Type == ObjectType.Sound) {
      if(obj.UseLightForVolume) {
        ret.Add("volume", obj.Volume);
      } else {
        ret.Add("volumeFromLight", obj.Light);
      }
      ret.Add("platformSound", obj.OnPlatform);
      ret.Add("floats", obj.Floats);
    } else {
      ret.Add("facing", obj.Facing);
      ret.Add("teleportsIn", obj.Invisible);
      ret.Add("teleportsOut", obj.Floats);
    }
    if(obj.PolygonIndex >= 0) {
      ret.Add("polygon", obj.PolygonIndex);
    } else {
      ret.Add("polygon", null);
    }
    ret.Add("x", obj.X);
    ret.Add("y", obj.Y);
    ret.Add("z", obj.Z);
    ret.Add("fromCeiling", obj.FromCeiling);
    ret.Add("networkOnly", obj.NetworkOnly);
    if(obj.Type == ObjectType.Monster) {
      ret.Add("activationBias", obj.ActivationBias.ToString());
      ret.Add("blind", obj.Blind);
      ret.Add("deaf", obj.Deaf);
    }
    return ret;
  }
  private static JObject JsonifyTextureDefinition(Side.TextureDefinition def, int transferMode, int lightsourceIndex) {
    if(def.Texture.IsEmpty()) return null;
    JObject ret = new JObject();
    ret.Add("x", def.X);
    ret.Add("y", def.Y);
    ret.Add("texture", JsonifyShapeDescriptor(def.Texture));
    ret.Add("transferMode", transferMode);
    ret.Add("light", lightsourceIndex);
    return ret;
  }
  private static JObject JsonifySide(Side side) {
    JObject ret = new JObject();
    ret.Add("type", side.Type.ToString());
    JObject primary = JsonifyTextureDefinition(side.Primary, side.PrimaryTransferMode, side.PrimaryLightsourceIndex);
    ret.Add("primary", primary);
    JObject secondary = JsonifyTextureDefinition(side.Secondary, side.SecondaryTransferMode, side.SecondaryLightsourceIndex);
    ret.Add("secondary", secondary);
    JObject transparent = JsonifyTextureDefinition(side.Transparent, side.TransparentTransferMode, side.TransparentLightsourceIndex);
    ret.Add("transparent", transparent);
    if(side.IsControlPanel) {
      JObject control = new JObject();
      control.Add("type", side.ControlPanelType);
      control.Add("permutation", side.ControlPanelPermutation);
      control.Add("initialState", (side.Flags & SideFlags.ControlPanelStatus) != 0);
      control.Add("isRepairSwitch", (side.Flags & SideFlags.IsRepairSwitch) != 0);
      control.Add("isDestructiveSwitch", (side.Flags & SideFlags.IsDestructiveSwitch) != 0);
      control.Add("controlPanelRequiresLight", (side.Flags & SideFlags.IsLightedSwitch) != 0);
      control.Add("isDestructibleSwitch", (side.Flags & SideFlags.SwitchCanBeDestroyed) != 0);
      control.Add("isProjectileSwitch", (side.Flags & SideFlags.SwitchCanOnlyBeHitByProjectiles) != 0);
      // left out: .Dirty
      ret.Add("controlPanel", control);
    } else ret.Add("controlPanel", null);
    ret.Add("polygonIndex", side.PolygonIndex);
    ret.Add("lineIndex", side.LineIndex);
    if(side.AmbientDelta != 0) ret.Add("ambientDelta", side.AmbientDelta);
    return ret;
  }
  private static JObject JsonifyPlatform(Platform platform) {
    JObject ret = new JObject();
    ret.Add("type", platform.Type.ToString());
    ret.Add("minimumHeight", platform.MinimumHeight);
    ret.Add("maximumHeight", platform.MaximumHeight);
    ret.Add("speed", platform.Speed);
    ret.Add("delay", platform.Delay);
    if(platform.Tag >= 0) ret.Add("tag", platform.Tag);
    else ret.Add("tag", null);
    /* This is a LOT of flags. One might be tempted to only include flags that
       are true, but considering the purpose of this plugin, I think it's more
       useful to maximize self-description by having every flag present. */
    ret.Add("initiallyActive", platform.InitiallyActive);
    ret.Add("initiallyExtended", platform.InitiallyExtended);
    ret.Add("deactivatesAtEachLevel", platform.DeactivatesAtEachLevel);
    ret.Add("deactivatesAtInitialLevel", platform.DeactivatesAtInitialLevel);
    ret.Add("activatesAdjacentPlatformsWhenDeactivating", platform.ActivatesAdjacentPlatformsWhenDeactivating);
    ret.Add("extendsFloorToCeiling", platform.ExtendsFloorToCeiling);
    ret.Add("comesFromFloor", platform.ComesFromFloor);
    ret.Add("comesFromCeiling", platform.ComesFromCeiling);
    ret.Add("causesDamage", platform.CausesDamage);
    ret.Add("doesNotActivateParent", platform.DoesNotActivateParent);
    ret.Add("activatesOnlyOnce", platform.ActivatesOnlyOnce);
    ret.Add("activatesLight", platform.ActivatesLight);
    ret.Add("deactivatesLight", platform.DeactivatesLight);
    ret.Add("isPlayerControllable", platform.IsPlayerControllable);
    ret.Add("isMonsterControllable", platform.IsMonsterControllable);
    ret.Add("reversesDirectionWhenObstructed", platform.ReversesDirectionWhenObstructed);
    ret.Add("cannotBeExternallyDeactivated", platform.CannotBeExternallyDeactivated);
    ret.Add("usesNativePolygonHeights", platform.UsesNativePolygonHeights);
    ret.Add("delaysBeforeActivation", platform.DelaysBeforeActivation);
    ret.Add("activatesAdjacentPlatformsWhenActivating", platform.ActivatesAdjacentPlatformsWhenActivating);
    ret.Add("deactivatesAdjacentPlatformsWhenActivating", platform.DeactivatesAdjacentPlatformsWhenActivating);
    ret.Add("deactivatesAdjacentPlatformsWhenDeactivating", platform.DeactivatesAdjacentPlatformsWhenDeactivating);
    ret.Add("activatesAdjacentPlatformsAtEachLevel", platform.ActivatesAdjacantPlatformsAtEachLevel);
    ret.Add("isLocked", platform.IsLocked);
    ret.Add("isSecret", platform.IsSecret);
    ret.Add("isDoor", platform.IsDoor);
    return ret;
  }
  private static JObject JsonifyPlacement(Placement plac) {
    JObject ret = new JObject();
    ret.Add("initialCount", plac.InitialCount);
    ret.Add("minimumCount", plac.MinimumCount);
    ret.Add("maximumCount", plac.MaximumCount);
    ret.Add("randomCount", plac.RandomCount);
    // I'd like to give the raw value, or a more accurate one, but oh well
    ret.Add("randomPercent", plac.RandomPercent);
    ret.Add("randomLocation", plac.RandomLocation);
    return ret;
  }
  private static JObject JsonifyAnnotation(Annotation ann) {
    JObject ret = new JObject();
    if(ann.Type != 0) ret.Add("type", ann.Type);
    ret.Add("x", ann.X);
    ret.Add("y", ann.Y);
    ret.Add("polygon", ann.PolygonIndex);
    ret.Add("text", ann.Text);
    return ret;
  }
  private static JObject JsonifyMedia(Media media) {
    JObject ret = new JObject();
    ret.Add("type", (int)media.Type);
    ret.Add("soundObstructedByFloor", media.SoundObstructedByFloor);
    ret.Add("controllingLight", media.LightIndex);
    ret.Add("directionOfCurrent", media.Direction);
    ret.Add("magnitudeOfCurrent", media.CurrentMagnitude);
    ret.Add("minimumLightIntensity", media.MinimumLightIntensity);
    ret.Add("highTide", media.High);
    ret.Add("lowTide", media.Low);
    return ret;
  }
  private static JObject JsonifyAmbientSound(AmbientSound snd) {
    JObject ret = new JObject();
    ret.Add("sound", snd.SoundIndex);
    ret.Add("volume", snd.Volume);
    // no flags?
    return ret;
  }
  private static JObject JsonifyRandomSound(RandomSound snd) {
    JObject ret = new JObject();
    ret.Add("sound", snd.SoundIndex);
    ret.Add("volume", snd.Volume);
    ret.Add("deltaVolume", snd.DeltaVolume);
    ret.Add("period", snd.Period);
    ret.Add("deltaPeriod", snd.DeltaPeriod);
    if(snd.NonDirectional) {
      ret.Add("nonDirectional", true);
    } else {
      ret.Add("direction", snd.Direction);
      ret.Add("deltaDirection", snd.DeltaDirection);
    }
    ret.Add("pitch", snd.Pitch);
    ret.Add("deltaPitch", snd.DeltaPitch);
    return ret;
  }
  private static JObject JsonifyMapInfo(Level level) {
    JObject ret = new JObject();
    ret.Add("name", level.Name);
    ret.Add("wallCollection", level.Environment);
    ret.Add("landscapeCollection", level.Landscape);
    JObject mission = new JObject();
    mission.Add("extermination", level.Extermination);
    mission.Add("exploration", level.Exploration);
    mission.Add("retrieval", level.Retrieval);
    mission.Add("repair", level.Repair);
    mission.Add("rescue", level.Rescue);
    mission.Add("explorationM1", level.ExplorationM1);
    mission.Add("rescueM1", level.RescueM1);
    mission.Add("repairM1", level.RepairM1);
    ret.Add("mission", mission);
    JObject environment = new JObject();
    environment.Add("vacuum", level.Vacuum);
    environment.Add("magnetic", level.Magnetic);
    environment.Add("rebellion", level.Rebellion);
    environment.Add("lowGravity", level.LowGravity);
    environment.Add("glueM1", level.GlueM1);
    environment.Add("ouchM1", level.OuchM1);
    environment.Add("rebellionM1", level.RebellionM1);
    environment.Add("songIndexM1", level.SongIndexM1);
    environment.Add("terminalsStopTime", level.TerminalsStopTime);
    environment.Add("m1ActivationRange", level.M1ActivationRange);
    environment.Add("m1Weapons", level.M1Weapons);
    // non-exposed environment flags: Network, SinglePlayer
    ret.Add("environment", environment);
    JObject entryPoints = new JObject();
    entryPoints.Add("solo", level.SinglePlayer);
    entryPoints.Add("coop", level.MultiplayerCooperative);
    entryPoints.Add("emfh", level.MultiplayerCarnage);
    entryPoints.Add("ktmwtb", level.KillTheManWithTheBall);
    entryPoints.Add("koth", level.KingOfTheHill);
    entryPoints.Add("defense", level.Defense);
    entryPoints.Add("rugby", level.Rugby);
    entryPoints.Add("ctf", level.CaptureTheFlag);
    ret.Add("entryPoints", entryPoints);
    return ret;
  }
  private static void ExportTheMap(Editor editor, string path) {
    Level level = editor.Level;
    JObject json = new JObject();
    JArray points = new JArray();
    for(int i = 0; i < level.Endpoints.Count; ++i) {
      points.Add(JsonifyPoint(level.Endpoints[i]));
    }
    json.Add("points", points);
    JArray lines = new JArray();
    for(int i = 0; i < level.Lines.Count; ++i) {
      lines.Add(JsonifyLine(level.Lines[i]));
    }
    json.Add("lines", lines);
    JArray polygons = new JArray();
    for(int i = 0; i < level.Polygons.Count; ++i) {
      polygons.Add(JsonifyPolygon(level.Polygons[i]));
    }
    json.Add("polygons", polygons);
    JArray objects = new JArray();
    for(int i = 0; i < level.Objects.Count; ++i) {
      objects.Add(JsonifyObject(level.Objects[i]));
    }
    json.Add("objects", objects);
    JArray sides = new JArray();
    for(int i = 0; i < level.Sides.Count; ++i) {
      sides.Add(JsonifySide(level.Sides[i]));
    }
    json.Add("sides", sides);
    JArray platforms = new JArray();
    for(int i = 0; i < level.Platforms.Count; ++i) {
      platforms.Add(JsonifyPlatform(level.Platforms[i]));
    }
    json.Add("platforms", platforms);
    JArray lights = new JArray();
    for(int i = 0; i < level.Lights.Count; ++i) {
      lights.Add(JsonifyLight(level.Lights[i]));
    }
    json.Add("lights", lights);
    JArray items = new JArray();
    for(int i = 0; i < level.ItemPlacement.Count; ++i) {
      items.Add(JsonifyPlacement(level.ItemPlacement[i]));
    }
    json.Add("itemPlacement", items);
    JArray monsters = new JArray();
    for(int i = 0; i < level.MonsterPlacement.Count; ++i) {
      monsters.Add(JsonifyPlacement(level.MonsterPlacement[i]));
    }
    json.Add("monsterPlacement", monsters);
    JArray annotations = new JArray();
    for(int i = 0; i < level.Annotations.Count; ++i) {
      annotations.Add(JsonifyAnnotation(level.Annotations[i]));
    }
    json.Add("annotations", annotations);
    JArray medias = new JArray();
    for(int i = 0; i < level.Medias.Count; ++i) {
      medias.Add(JsonifyMedia(level.Medias[i]));
    }
    json.Add("medias", medias);
    JArray ambientSounds = new JArray();
    for(int i = 0; i < level.AmbientSounds.Count; ++i) {
      ambientSounds.Add(JsonifyAmbientSound(level.AmbientSounds[i]));
    }
    json.Add("ambientSounds", ambientSounds);
    JArray randomSounds = new JArray();
    for(int i = 0; i < level.RandomSounds.Count; ++i) {
      randomSounds.Add(JsonifyRandomSound(level.RandomSounds[i]));
    }
    json.Add("randomSounds", randomSounds);
    json.Add("mapInfo", JsonifyMapInfo(level));
    using(StreamWriter writer = new StreamWriter(path)) {
      writer.WriteLine(JsonConvert.SerializeObject(json, Formatting.Indented));
    }
  }
  public static void GtkRun(Editor editor) {
    Window window = null; // TODO: editor window
    FileChooserDialog d = new FileChooserDialog(
      "Export JSON as",
      window,
      FileChooserAction.Save,
      "Cancel", ResponseType.Cancel, "Save", ResponseType.Accept
    );
    d.SetCurrentFolder(lastExportFolder ?? Weland.Weland.Settings.GetSetting("LastSave/Folder", Environment.GetFolderPath(Environment.SpecialFolder.Personal)));
    d.CurrentName = editor.Level.Name + ".json";
    d.DoOverwriteConfirmation = true;
    try {
      if(d.Run() == (int)ResponseType.Accept) {
        ExportTheMap(editor, d.Filename);
        lastExportFolder = Path.GetDirectoryName(d.Filename);
      }
    } catch(Exception e) {
      MessageDialog m = new MessageDialog(
        window,
        DialogFlags.DestroyWithParent,
        MessageType.Error,
        ButtonsType.Close,
        "An error occurred while exporting."
      );
      m.Title = "JSON Export Error";
      m.SecondaryText = e.Message;
      m.Run();
      m.Destroy();
    }
    d.Destroy();
  }
}
