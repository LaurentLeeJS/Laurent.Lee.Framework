# Laurent.Lee.Framework

C# is used to organize and implement a set of development framework class libraries, which is not built on the UI Application (due to the recent development of the research based on X OS, so the update is not very frequent). Follow the GNU protocol. In the other CLB.Expends I also made a statement of the open source (for commercial use).

The framework itself is based on the latest Framework.NET 4.6, but I do not have the feature support for WPF, the Comman attribute is the mutual operation mode, supports Unsafe mode, the framework itself does not add code analysis (my VS2015 is IDE, itself integrated analyzer).

The framework includes: TCP communications, memory database engine, HTTP services, front and rear end integrated Web view framework, ORM caching, binary serialization, JSON serialization, chain programming API, word segmentation search, HTML parser, LAN monitoring, GIF file processing, WEB Title crawl, Chinese encoding recognition, UDP penetration sample, third party platform OpenAPI.

Which notes:

#TCP communication

Function based on the prototype of the call, only need to add the Attribute configuration to the local function, you can generate a consistent with the local function of the remote agent.
Supports two types of single case service and cross type (non supporting cross program set).
Support for stable binary serialization, support for the non stable JSON serialization, and compatible with HTTP call.
Selection of synchronous and asynchronous mode and flexibility.
The realization of client connection single batch mode.

#Web view

Automatically generate the client view object based on the server side view object matching HTML template.
II using client data object driven UI display, the operation is simple and clear.
HTML template as a data filter, filter out the garbage data, from the fundamental solution to the problem of data object cycle reference.
The HTML template definition is simple, basic operation only supports, such as Loop/Value/If/Not/At.
The HTML support member is bound to a client function.
The support for the client at the object level reference identification.
The query server support automatic identification parameter types.

#ORM+ cache

ORM only supports a single table operation, but support the details of the operation, support Expression Lambda.
ORM supports the natural and flexible Model field type, supporting composite data type (combination mode).
ORM supports inheritance, Model and Table 1 to achieve more.
To provide a variety of commonly used cache mode, support custom cache mode.
The multi table operation recommended cache mode, after all, cheaper than artificial memory optimization.

Interested Geeker/Hacker/Programmer can be feedback to me, or contact me to join this framework of the GitHub development group (due to the realization of their own, at present, a person);
If you join the team. Best if you have some programming skills, prior to the submission of the debug about, never submit garbage code, so as not to harm other open source enthusiasts (I do not have so much energy to audit other people's code);
Want to achieve together can give my email: Gmail:LaurentLeeJS@Gmail.com
