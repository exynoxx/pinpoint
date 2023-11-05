import QtQuick 2.12
import QtQuick.Window 2.12
import QtQuick.Controls 2.12
import QtQuick.Layouts 1.3

ApplicationWindow {
    id: mainWindow
    visible: true
    width: 800
    height: 230
    color: "transparent"
    flags: Qt.FramelessWindowHint

    signal updatedResultSignal(var result);

    Rectangle {
        width: mainWindow.width
        height: mainWindow.height
        color: "#FF1C2228"
        radius: 10

        ColumnLayout {
            anchors.fill: parent
            Layout.fillWidth: true
            Layout.alignment: Qt.AlignTop

            Rectangle {
                Layout.fillWidth: true
                Layout.alignment: Qt.AlignTop
                Layout.topMargin: 10
                color: "#FF1C2228"
                //color: "grey"
                height: 60
                TextInput {
                    id: inputField
                    anchors.fill: parent
                    font.pixelSize: 20
                    color: "white"
                    //color: "black"
                    padding: 20                   
                    onTextChanged: {
                        TextChangedHandler.handle(inputField.text);
                    }
                }
            }

            ListView {
                id: listView
                clip: true
                interactive: false
                Layout.fillWidth: true
                Layout.fillHeight: true
                height: contentHeight
                model: ListModel {
                    ListElement { name: "Result 1" }
                    ListElement { name: "Result 2" }
                    ListElement { name: "Result 3" }
                    // Add more results as needed
                }
                delegate: Item {
                    width: parent.width
                    height: 50
                    Rectangle {
                        width: parent.width
                        height: 50
                        color: "#FF1C2228"
                        //color: index % 2 === 0 ? "lightgray" : "darkgrey"
                        //border.color: "lightgray"
                        //border.width: 1
                        Text {
                            anchors.left: parent.left
                            text: model.name
                            color: "white"
                            font.pixelSize: 20
                            leftPadding: 20
                            padding: 15
                        }
                    }
                }
            }

        }
    }

    Connections {
        target: TextChangedHandler // This refers to the C++ TextChangedHandler instance
        onUpdatedResultSignal: {
            // Update the ListView model with the new data
            listView.model.clear(); // Clear the existing data
            for (var i = 0; i < result.length; i++) {
                listView.model.append({ name: result[i] });
            }
        }
    }

}
