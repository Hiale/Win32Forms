#define WIN32_LEAN_AND_MEAN
#include <Windows.h>
#include <tchar.h>
#include <memory>
#include "resource.h"
#include "ControlResizer.h" //Add header

#pragma comment(linker, \
  "\"/manifestdependency:type='Win32' "\
  "name='Microsoft.Windows.Common-Controls' "\
  "version='6.0.0.0' "\
  "processorArchitecture='*' "\
  "publicKeyToken='6595b64144ccf1df' "\
  "language='*'\"")

std::shared_ptr<ControlResizer> resizer;

INT_PTR CALLBACK DialogProc(HWND hDlg, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	switch (uMsg)
	{
	case WM_INITDIALOG:
		resizer = std::make_shared<ControlResizer>(hDlg); //Create instance

		//Add controls with their anchor style.
		//If the control is within a container (for example Group Box) use the overloaded method.
		resizer->addControl(BTNCONVERT, AnchorStyle::Bottom | AnchorStyle::Right);
		resizer->addControl(GRPOUTPUT, AnchorStyle::Bottom | AnchorStyle::Left | AnchorStyle::Right);
		resizer->addControl(CHKRESIZE, GRPOUTPUT, AnchorStyle::Top | AnchorStyle::Left);
		resizer->addControl(CHKREPLACE, GRPOUTPUT, AnchorStyle::Top | AnchorStyle::Left);
		resizer->addControl(CHKUSECONTROLNAME, GRPOUTPUT, AnchorStyle::Top | AnchorStyle::Left);
		resizer->addControl(BTNBROWSERESOURCE, GRPOUTPUT, AnchorStyle::Top | AnchorStyle::Right);
		resizer->addControl(TXTRESOURCE, GRPOUTPUT, AnchorStyle::Top | AnchorStyle::Left | AnchorStyle::Right);
		resizer->addControl(GRPINPUT, AnchorStyle::Top | AnchorStyle::Bottom | AnchorStyle::Left | AnchorStyle::Right);
		resizer->addControl(LSTFORMS, GRPINPUT, AnchorStyle::Top | AnchorStyle::Bottom | AnchorStyle::Left | AnchorStyle::Right);
		resizer->addControl(BTNBROWSEASSEMBLY, GRPINPUT, AnchorStyle::Top | AnchorStyle::Right);
		resizer->addControl(TXTASSEMBLY, GRPINPUT, AnchorStyle::Top | AnchorStyle::Left | AnchorStyle::Right);
		break;
	case WM_COMMAND:
		switch (LOWORD(wParam))
		{
		case IDCANCEL:
			SendMessage(hDlg, WM_CLOSE, 0, 0);
			break;
		default:
			return FALSE;
		}
		break;
	case WM_CLOSE:
		DestroyWindow(hDlg);
		break;
	case WM_DESTROY:
		PostQuitMessage(0);
		break;
	case WM_SIZE:
		resizer->onResize(); //Process WM_SIZE message
		break;
	default:
		return FALSE;
	}
	return TRUE;
}

int WINAPI _tWinMain(HINSTANCE hInst, HINSTANCE h0, LPTSTR lpCmdLine, int nCmdShow)
{
	HWND hDlg;
	MSG msg;

	hDlg = CreateDialogParam(hInst, MAKEINTRESOURCE(IDD_MAINFORM), 0, DialogProc, 0); //Replace IDD_MAINFORM with Dialog ID
	ShowWindow(hDlg, nCmdShow);

	while (GetMessage(&msg, 0, 0, 0)) {
		if (!IsDialogMessage(hDlg, &msg)) {
			TranslateMessage(&msg);
			DispatchMessage(&msg);
		}
	}
	return 0;
}