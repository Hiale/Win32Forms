//
// Copyright (c) 2016 Hiale
//
// Distributed under MIT License. (See https://opensource.org/licenses/MIT)
//
//
//
// Generated by

#include "ControlResizer.h"

void ControlResizer::init(HWND hWnd)
{
	this->hWnd = hWnd;
	GetClientRect(hWnd, &rectOld);
}

void ControlResizer::addControl(HWND hwndCtl, const AnchorStyle& anchorStyle)
{
	RECT rect;
	GetWindowRect(hwndCtl, &rect);
	MapWindowPoints(HWND_DESKTOP, hWnd, (LPPOINT)&rect, 2);
	ControlResizerItem item;
	item.hWnd = hwndCtl;
	item.anchorStyle = anchorStyle;
	item.rect = rect;
	controls[hwndCtl] = item;
}

void ControlResizer::addControl(int ctlId, const AnchorStyle& anchorStyle)
{
	addControl(GetDlgItem(hWnd, ctlId), anchorStyle);
}

void ControlResizer::removeControl(HWND hwndCtl)
{
	controls.erase(hwndCtl);
}

void ControlResizer::removeControl(int ctlId)
{
	removeControl(GetDlgItem(hWnd, ctlId));
}

void ControlResizer::removeAll()
{
	controls.clear();
}

void ControlResizer::onResize()
{
	RECT rect;
	GetClientRect(hWnd, &rect);
	for (auto& control : controls)
	{
		ControlResizerItem& item = control.second;
		//horizontal		
		if ((item.anchorStyle & AnchorStyle::Left) == AnchorStyle::Left && (item.anchorStyle & AnchorStyle::Right) == AnchorStyle::Right)
		{			
			//left and right is set --> change width
			long diff = rect.right - rectOld.right;
			item.rect.right += diff;
		}
		else if ((item.anchorStyle & AnchorStyle::Left) == AnchorStyle::Left)
		{
			//only left is set --> nothing, left is default
		}
		else if ((item.anchorStyle & AnchorStyle::Right) == AnchorStyle::Right)
		{
			//only right is set --> move right
			long diff = rect.right - rectOld.right;
			item.rect.left += diff;
			item.rect.right += diff;
		}
		else
		{
			//neither left nor right is set --> move relatively
			long diff = ((rect.right - rect.left) / 2) - ((rectOld.right - rectOld.left) / 2);
			item.rect.left += diff;
			item.rect.right += diff;
		}
		//vertical
		if ((item.anchorStyle & AnchorStyle::Top) == AnchorStyle::Top && (item.anchorStyle & AnchorStyle::Bottom) == AnchorStyle::Bottom)
		{
			//top and bottom is set --> change height
			item.rect.bottom += rect.bottom - rectOld.bottom;
		}
		else if ((item.anchorStyle & AnchorStyle::Top) == AnchorStyle::Top)
		{
			//only top is set --> nothing, top is default
		}
		else if ((item.anchorStyle & AnchorStyle::Bottom) == AnchorStyle::Bottom)
		{
			//only bottom is set --> move bottom
			long diff = rect.bottom - rectOld.bottom;
			item.rect.top += diff;
			item.rect.bottom += diff;			
		}
		else //none
		{
			//neither top nor bottm is set --> move relatively
			long diff = ((rect.bottom - rect.top) / 2) - ((rectOld.bottom - rectOld.top) / 2);
			item.rect.top += diff;
			item.rect.bottom += diff;
		}
		MoveWindow(item.hWnd, item.rect.left, item.rect.top, item.rect.right - item.rect.left, item.rect.bottom - item.rect.top, TRUE);
	}
	rectOld = rect;
}