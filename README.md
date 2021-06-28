<div class="row">
  <div class="column">

  </div>
  <div class="column">

  </div>
</div>
<p float="left">
    <a href="https://revolve.no/"><img align=left src="https://raw.githubusercontent.com/RevolveNTNU/RevolveUavcan/tribe/.github/main/revolve.svg" width="30%"/></a>
    <a href="https://uavcan.org/"><img align=right margin src="https://raw.githubusercontent.com/RevolveNTNU/RevolveUavcan/tribe/.github/main/uavcan.svg" width="30%"/></a>
</p>
<br>
<br>

# Cross-platform Uavcan client
A desktop application, based on the Avalonia framework, serving as a demo on how to use [Revolve Uavcan](https://github.com/RevolveNTNU/RevolveUavcan). The app is a client for sending and receiving UAVCAN messages over a UDP connection. Services have not been implemented yet, but will be coming soon. In the release section of this repository you can find executables for Windows and Linux.

## User guide
A quick walkthrough on how to use this application. It can both send and receive UAVCAN messages over UDP.

### Start-up
The application opens with a popup asking the user for some basic settings: 

- Path to the DSDL folder that should be used
- IP Address for the UDP connection
- Port to read from
- Port to write to

<img align=left src="https://github.com/RevolveNTNU/CrossPlatformUavcanClient/raw/tribe/.github/readme/setup.png" width="100%"/>

The folder you select for the DSDL path should be the parent folder to your DSDL. The selected folder (revolve_dsdl) will not be a part of the namespace definition for your messages. See image below. "ams" will be the root namespace for messages inside.

<img align=left src="https://github.com/RevolveNTNU/CrossPlatformUavcanClient/raw/tribe/.github/readme/choose_dsdl_folder.png" width="100%"/>

### Namespace selection

All messages present in the selected DSDL can be sent using the client. They are sorted my namespace. Using the top-left dropdown menu, you can select a namespace to locate the message you want to send.

<img align=left src="https://github.com/RevolveNTNU/CrossPlatformUavcanClient/raw/tribe/.github/readme/namespace_selection.png" width="100%"/>

### Sending a message
Once you have located your message, you can provide data values for the fields and the source node ID. These are entered in their respective fields for a chosen message. The messages are sent over UDP, to the selected write port. The message format is as follows:

```
udp_payload = {long timestamp, uint32 canid, uint8 datalenght, uint8[datalenght-1] data, uint8 tailbyte}
```
Note: The timestamp and CAN ID are sent in network endianess. The CAN ID needs to have a 1 on bit 31, to indicate that it is in extendend frame format:

```
can_id = {1 0 0 bit[29] can_id}
```

<img align=left src="https://github.com/RevolveNTNU/CrossPlatformUavcanClient/raw/tribe/.github/readme/send_message.png" width="100%"/>

### Receiving messages
UDP packages sent to the reading port are attempted to be parsed. If the extended frame bit is 1, it will be passed on to the UAVCAN Parser. As of now, the read and write format is different, due to compatibility with Revolve Analyze. This might get changed later on. The read format is as follows:

```
udp_payload = {uint8 datalenght, uint32 canid, uint8[datalenght-1] data, uint8 tailbyte}
```
Note: The CAN ID is sent in network endianess. The CAN ID needs to have a 1 on bit 31, to indicate that it is in extendend frame format:

```
can_id = {1 0 0 bit[29] can_id}
```
<img align=left src="https://github.com/RevolveNTNU/CrossPlatformUavcanClient/raw/tribe/.github/readme/receive_message.png" width="100%"/>

