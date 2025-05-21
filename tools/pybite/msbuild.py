import xml.etree.ElementTree as ET
from typing import Optional, Dict, List

class MSBuildElement:
    """
    Base wrapper for an XML element in an MSBuild file.
    Provides attribute and child element lookup.
    """
    def __init__(self, element: ET.Element):
        self._element = element

    def __getattr__(self, name: str):
        # Try attribute first, then child element
        if name in self._element.attrib:
            return self._element.attrib[name]
        child = self._element.find(name)
        if child is not None:
            return child.text
        raise AttributeError(f"{self.__class__.__name__} has no attribute '{name}'")

    @property
    def tag(self) -> str:
        """Returns the XML tag name."""
        return self._element.tag

    @property
    def attrib(self) -> Dict[str, str]:
        """Returns the XML attributes as a dict."""
        return self._element.attrib

    @property
    def text(self) -> Optional[str]:
        """Returns the text content of the element."""
        return self._element.text

class MSBuildTarget(MSBuildElement):
    """
    Represents an MSBuild <Target> element.
    Exposes common attributes and child elements as properties.
    """
    @property
    def Name(self) -> Optional[str]:
        """The Name attribute of the target."""
        return self._element.attrib.get("Name")

    @property
    def DependsOnTargets(self) -> Optional[str]:
        """The DependsOnTargets attribute."""
        return self._element.attrib.get("DependsOnTargets")

    @property
    def Inputs(self) -> Optional[str]:
        """The Inputs attribute."""
        return self._element.attrib.get("Inputs")

    @property
    def Outputs(self) -> Optional[str]:
        """The Outputs attribute."""
        return self._element.attrib.get("Outputs")

    @property
    def Condition(self) -> Optional[str]:
        """The Condition attribute."""
        return self._element.attrib.get("Condition")

    @property
    def BeforeTargets(self) -> Optional[str]:
        """The BeforeTargets attribute."""
        return self._element.attrib.get("BeforeTargets")

    @property
    def AfterTargets(self) -> Optional[str]:
        """The AfterTargets attribute."""
        return self._element.attrib.get("AfterTargets")

    @property
    def Returns(self) -> Optional[str]:
        """The Returns attribute."""
        return self._element.attrib.get("Returns")

    @property
    def KeepDuplicateOutputs(self) -> Optional[str]:
        """The KeepDuplicateOutputs attribute."""
        return self._element.attrib.get("KeepDuplicateOutputs")

    @property
    def Tasks(self) -> List[MSBuildElement]:
        """
        Returns a list of child task elements (all direct children that are not PropertyGroup/ItemGroup/OnError).
        """
        return [MSBuildElement(e) for e in self._element if e.tag not in ("PropertyGroup", "ItemGroup", "OnError")]

    @property
    def OnError(self) -> List[MSBuildElement]:
        """
        Returns a list of <OnError> child elements.
        """
        return [MSBuildElement(e) for e in self._element.findall("OnError")]

class MSBuildProperty(MSBuildElement):
    """
    Represents a property element in MSBuild.
    """
    @property
    def name(self) -> str:
        """The property name (tag)."""
        return self._element.tag

    @property
    def value(self) -> Optional[str]:
        """The property value (text)."""
        return self._element.text

class MSBuildItem(MSBuildElement):
    """
    Represents an item element in MSBuild.
    """
    @property
    def Include(self) -> Optional[str]:
        """The Include attribute."""
        return self._element.attrib.get("Include")

    @property
    def Remove(self) -> Optional[str]:
        """The Remove attribute."""
        return self._element.attrib.get("Remove")

    @property
    def Update(self) -> Optional[str]:
        """The Update attribute."""
        return self._element.attrib.get("Update")

    @property
    def name(self) -> str:
        """The item name (tag)."""
        return self._element.tag

class MSBuildFile:
    """
    Represents an MSBuild file and provides access to its structure.
    """
    def __init__(self, filepath: Optional[str] = None, xml_string: Optional[str] = None):
        """
        Initialize the MSBuildProject from a file or XML string.

        :param filepath: Path to the MSBuild XML file.
        :param xml_string: XML content as a string.
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
        Returns a list of <PropertyGroup> elements.
        """
        return self.root.findall(".//PropertyGroup")

    def get_item_groups(self) -> List[ET.Element]:
        """
        Returns a list of <ItemGroup> elements.
        """
        return self.root.findall(".//ItemGroup")

    def get_properties(self) -> Dict[str, MSBuildProperty]:
        """
        Returns a dict of all properties in all <PropertyGroup>s as MSBuildProperty objects.
        """
        props: Dict[str, MSBuildProperty] = {}
        for pg in self.get_property_groups():
            for prop in pg:
                props[prop.tag] = MSBuildProperty(prop)
        return props

    def get_items(self) -> Dict[str, List[MSBuildItem]]:
        """
        Returns a dict of item type to list of MSBuildItem objects in all <ItemGroup>s.
        """
        items: Dict[str, List[MSBuildItem]] = {}
        for ig in self.get_item_groups():
            for item in ig:
                items.setdefault(item.tag, []).append(MSBuildItem(item))
        return items

    def get_targets(self) -> List[MSBuildTarget]:
        """
        Returns a list of MSBuildTarget objects for <Target> elements.
        """
        return [MSBuildTarget(e) for e in self.root.findall(".//Target")]
