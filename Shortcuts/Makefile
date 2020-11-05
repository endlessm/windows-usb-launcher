# Couldn't find a way to split lines in nmake
CFLAGS = /c /nologo /W3 /WX- /sdl /O2 /Oi /GS /Gy /permissive- /TP /FC /Zc:wchar_t /Zc:forScope /Zc:inline /errorReport:prompt /D NDEBUG /D _WINDOWS /D _UNICODE /D UNICODE
LDFLAGS = /ERRORREPORT:PROMPT /INCREMENTAL:NO /NOLOGO kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib /MANIFEST /MANIFESTUAC:"level='asInvoker' uiAccess='false'" /manifest:embed /SUBSYSTEM:WINDOWS /OPT:REF /OPT:ICF /LTCG:incremental /TLBID:1 /DYNAMICBASE /NXCOMPAT /MACHINE:X64

all: $(MAKE_OUTDIR)/KiwixShortcut.exe $(MAKE_OUTDIR)/KolibriShortcut.exe

$(MAKE_OUTDIR)/KiwixShortcut.exe: $(MAKE_INTDIR)/KiwixShortcut.obj $(MAKE_INTDIR)/KiwixShortcut.res
	link.exe $(LDFLAGS) /OUT:"$@" $**

$(MAKE_INTDIR)/KiwixShortcut.obj: main.c
	cl $(CFLAGS) /D "WUL_LAUNCHER_FLAGS=\"--kiwix\"" /Fo"$@" $**

$(MAKE_INTDIR)/KiwixShortcut.res: resource.rc
	rc /nologo /d "WUL_ICON_FILENAME=kiwix.ico" /fo"$@" $**

$(MAKE_OUTDIR)/KolibriShortcut.exe: $(MAKE_INTDIR)/KolibriShortcut.obj $(MAKE_INTDIR)/KolibriShortcut.res
	link.exe $(LDFLAGS) /OUT:"$@" $**

$(MAKE_INTDIR)/KolibriShortcut.obj: main.c
	cl $(CFLAGS) /D "WUL_LAUNCHER_FLAGS=\"--kolibri\"" /Fo"$@" $**

$(MAKE_INTDIR)/KolibriShortcut.res: resource.rc
	rc /nologo /d "WUL_ICON_FILENAME=kolibri.ico" /fo"$@" $**

.PHONY: clean

clean:
	rmdir /S /Q obj
	rmdir /S /Q bin
