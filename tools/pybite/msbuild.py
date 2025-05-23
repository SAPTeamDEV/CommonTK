import xml.etree.ElementTree as ET
from typing import Optional, Dict, List

class MSBuildElement:
    """
    Wraps an XML element from an MSBuild file, providing attribute and child element access.
    """
    def __init__(self, element: ET.Element):
        """
        Initialize with an ElementTree element.
        """
        self._element = element

    def __getattr__(self, name: str):
        """
        Returns the value of an attribute or child element by name.
        Raises AttributeError if not found.
        """
        if name in self._element.attrib:
            return self._element.attrib[name]
        child = self._element.find(name)
        if child is not None:
            return child.text
        raise AttributeError(f"{self.__class__.__name__} has no attribute '{name}'")

    @property
    def tag(self) -> str:
        """
        Returns the XML tag name of the element.
        """
        return self._element.tag

    @property
    def attrib(self) -> Dict[str, str]:
        """
        Returns the element's attributes as a dictionary.
        """
        return self._element.attrib

    @property
    def text(self) -> Optional[str]:
        """
        Returns the text content of the element, or None.
        """
        return self._element.text

class MSBuildTarget(MSBuildElement):
    """
    Represents an MSBuild <Target> element, exposing common attributes and child tasks.
    """
    @property
    def Name(self):
        """Returns the Name attribute of the target."""
        return self._element.attrib.get("Name")
    @property
    def DependsOnTargets(self):
        """Returns the DependsOnTargets attribute."""
        return self._element.attrib.get("DependsOnTargets")
    @property
    def Inputs(self):
        """Returns the Inputs attribute."""
        return self._element.attrib.get("Inputs")
    @property
    def Outputs(self):
        """Returns the Outputs attribute."""
        return self._element.attrib.get("Outputs")
    @property
    def Condition(self):
        """Returns the Condition attribute."""
        return self._element.attrib.get("Condition")
    @property
    def BeforeTargets(self):
        """Returns the BeforeTargets attribute."""
        return self._element.attrib.get("BeforeTargets")
    @property
    def AfterTargets(self):
        """Returns the AfterTargets attribute."""
        return self._element.attrib.get("AfterTargets")
    @property
    def Returns(self):
        """Returns the Returns attribute."""
        return self._element.attrib.get("Returns")
    @property
    def KeepDuplicateOutputs(self):
        """Returns the KeepDuplicateOutputs attribute."""
        return self._element.attrib.get("KeepDuplicateOutputs")
    @property
    def Tasks(self):
        """
        Returns a list of child task elements (excluding PropertyGroup, ItemGroup, OnError).
        """
        return [MSBuildElement(e) for e in self._element if e.tag not in ("PropertyGroup", "ItemGroup", "OnError")]
    @property
    def OnError(self):
        """
        Returns a list of <OnError> child elements.
        """
        return [MSBuildElement(e) for e in self._element.findall("OnError")]

class MSBuildProperty(MSBuildElement):
    """
    Represents a property element in an MSBuild file.
    """
    @property
    def name(self):
        """Returns the property name (XML tag)."""
        return self._element.tag
    @property
    def value(self):
        """Returns the property value (text content)."""
        return self._element.text

class MSBuildItem(MSBuildElement):
    """
    Represents an item element in an MSBuild file.
    """
    @property
    def Include(self):
        """Returns the Include attribute."""
        return self._element.attrib.get("Include")
    @property
    def Remove(self):
        """Returns the Remove attribute."""
        return self._element.attrib.get("Remove")
    @property
    def Update(self):
        """Returns the Update attribute."""
        return self._element.attrib.get("Update")
    @property
    def name(self):
        """Returns the item name (XML tag)."""
        return self._element.tag

class MSBuildFile:
    """
    Loads and represents an MSBuild XML file, providing access to property groups, item groups, and targets.
    """
    def __init__(self, filepath: Optional[str] = None, xml_string: Optional[str] = None):
        """
        Initialize the MSBuildFile from a file path or XML string.

        :param filepath: Path to the MSBuild XML file.
        :param xml_string: XML content as a string.
        :raises ValueError: If neither filepath nor xml_string is provided.
        """
        if filepath:
            self.tree = ET.parse(filepath)
            self.root = self.tree.getroot()
        elif xml_string:
            self.root = ET.fromstring(xml_string)
            self.tree = ET.ElementTree(self.root)
        else:
            raise ValueError("Either filepath or xml_string must be provided.")

    def get_property_groups(self) -> List[ET.Element]:
        """
        Returns all <PropertyGroup> elements in the MSBuild file.
        """
        return self.root.findall(".//PropertyGroup")

    def get_item_groups(self) -> List[ET.Element]:
        """
        Returns all <ItemGroup> elements in the MSBuild file.
        """
        return self.root.findall(".//ItemGroup")

    def get_properties(self) -> Dict[str, MSBuildProperty]:
        """
        Returns a dictionary of all properties in all <PropertyGroup>s as MSBuildProperty objects.
        """
        props = {}
        for pg in self.get_property_groups():
            for prop in pg:
                props[prop.tag] = MSBuildProperty(prop)
        return props

    def get_items(self) -> Dict[str, List[MSBuildItem]]:
        """
        Returns a dictionary mapping item type to a list of MSBuildItem objects in all <ItemGroup>s.
        """
        items = {}
        for ig in self.get_item_groups():
            for item in ig:
                items.setdefault(item.tag, []).append(MSBuildItem(item))
        return items

    def get_targets(self) -> List[MSBuildTarget]:
        """
        Returns a list of MSBuildTarget objects for all <Target> elements in the file.
        """
        return [MSBuildTarget(e) for e in self.root.findall(".//Target")]
