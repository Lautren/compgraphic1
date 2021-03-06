// dx2dll.cpp: определяет экспортированные функции для приложения DLL.
//

//#include "stdafx.h"
//#include <Windows.h>


#include <d2d1.h>
#include <d2d1_1.h>
#include <vector>
/*
__declspec(dllexport) void hello()
{
	int x = 100;
}

ID2D1Factory* factory;
ID2D1HwndRenderTarget* target;


__declspec(dllexport)  bool Initialize(HWND handle)
{
//	currentLine->color = D2D1::ColorF(D2D1::ColorF::Red);
//	color = D2D1::ColorF(D2D1::ColorF::Red);
	RECT rect;
	if (!GetClientRect(handle, &rect)) return false;

	if (FAILED(D2D1CreateFactory(D2D1_FACTORY_TYPE_SINGLE_THREADED, &factory)))
		return false;

	return SUCCEEDED(factory->CreateHwndRenderTarget(D2D1::RenderTargetProperties(),
		D2D1::HwndRenderTargetProperties(handle, D2D1::SizeU(rect.right - rect.left,
			rect.bottom - rect.top)), &target));
}

__declspec(dllexport) void Close()
{
	if (factory) factory->Release();
	if (target) target->Release();
//	for (int i = 0; i<lines.size(); i++)
//	{
//		delete lines[i];
//	}
//	delete currentLine;
}
*/
using namespace System;

namespace lib
{
	// этот класс содержит ломаную из точек и цвета
	class Line {
	public: std::vector<D2D1_POINT_2F> points;
			D2D1_COLOR_F color,gc1,gc2;
			int style;
			bool gradient;
			void Render(ID2D1HwndRenderTarget* target);
			Line() { color = D2D1::ColorF(D2D1::ColorF::Black); gradient = false; style = 0; }
	};

	class Renderer
	{
	public:
		D2D1_COLOR_F safeColor,background;
		std::vector<Line*> lines;
		Line* currentLine;
		D2D1_POINT_2F *currentpoint;
		bool isChangePoint;
		~Renderer()
		{
			if (factory) factory->Release();
			if (target) target->Release();
			for (int i = 0; i<lines.size(); i++)
			{
				delete lines[i];
			}
			delete currentLine;
		}

		Renderer()
		{
			currentLine = new Line;
			isChangePoint = false;
		}


		bool Initialize(HWND handle)
		{
			currentLine->color = D2D1::ColorF(D2D1::ColorF::Red);
			color = D2D1::ColorF(D2D1::ColorF::Red);
			RECT rect;
			if (!GetClientRect(handle, &rect)) return false;

			if (FAILED(D2D1CreateFactory(D2D1_FACTORY_TYPE_SINGLE_THREADED, &factory)))
				return false;

			return SUCCEEDED(factory->CreateHwndRenderTarget(D2D1::RenderTargetProperties(),
				D2D1::HwndRenderTargetProperties(handle, D2D1::SizeU(rect.right - rect.left,
					rect.bottom - rect.top)), &target));
		}


		void NewPoly()
		{
			//Line l=currentLine;
			lines.push_back(currentLine);
			currentLine = new Line;
			currentLine->points.clear();
			currentLine->color = D2D1::ColorF(D2D1::ColorF::Red);
		}
		D2D1_COLOR_F color;
		void SetColor(unsigned  icolor)
		{
			currentLine->color = D2D1::ColorF(icolor);
			color = D2D1::ColorF(icolor);
			currentLine->gradient = false;
		}
		void SetGradient(unsigned int color1, unsigned int color2, unsigned int style)
		{
			currentLine->gc1 = D2D1::ColorF(color1);
			currentLine->gc2 = D2D1::ColorF(color2);
			currentLine->style = style;
			currentLine->gradient = true;
		}

		void RestorePolyColor()
		{
			currentLine->color = safeColor;
		}
		std::vector<D2D1_POINT_2F> points;
		void AddPoint(int x, int y)
		{
			D2D1_POINT_2F p = { x,y };
			if (!checkintersect(currentLine, p))
			{
				currentLine->points.push_back(p);
				points.push_back(p);
			}
		}

		void DeletePoly()
		{
			currentLine->points.clear();
			points.clear();
		}
		void DeletePoint()
		{
			currentLine->points.pop_back();
			points.pop_back();
		}

		void ChangePoint(int x, int y)
		{
			// повторное нажатие мыши при выборе точки указывает место куда ее поставить
			if (isChangePoint)
			{
				currentpoint->x = x;
				currentpoint->y = y;
				isChangePoint = false;
				currentLine->color = safeColor;
				safeColor = background;
				return;
			}
			// обработка первого нажатия мыши при выборе точки
			lines.push_back(currentLine);
			float minD = 10000;
			D2D1_POINT_2F  c = { x,y };
			std::vector<Line*>::iterator nearL;
			for (std::vector<Line*>::iterator i = lines.begin(); i != lines.end(); i++)
			{
				Line &p = **i;// ссылка на объект на который указывает итератор
				for (int l = 0; l< p.points.size(); l++)
					//std::vector<D2D1_POINT_2F>::iterator l=(**i).points.begin();l!=(**i).points.end();l+0+

				{
					float d = distance(p.points[l], c);
					if (d<minD)
					{
						minD = d; // самое короткое расстояние
						nearL = i;// нашли ближний итератор - линию
						currentpoint = &p.points[l]; // это ближайшая точка
					}
				}
			}
			currentLine = *nearL;
			safeColor = currentLine->color;
			currentLine->color = D2D1::ColorF(D2D1::ColorF::White);
			lines.erase(nearL);
			isChangePoint = true;
		}


		void ChangePoly(int x, int y)
		{
			lines.push_back(currentLine);
			float minD = 10000;
			D2D1_POINT_2F  c = { x,y };
			std::vector<Line*>::iterator nearL;
			for (std::vector<Line*>::iterator i = lines.begin(); i != lines.end(); i++)
			{
				Line &p = **i;// ссылка на объект на который указывает итератор
				for (int l = 0; l< p.points.size() - 1; l++)
					//std::vector<D2D1_POINT_2F>::iterator l=(**i).points.begin();l!=(**i).points.end();l+0+

				{
					float d = distance(p.points[l], p.points[l + 1], c);
					if (d<minD)
					{
						minD = d; // самое короткое расстояние
						nearL = i;// нашли ближний итератор - линию						 
					}
				}
			}
			currentLine = *nearL;
			safeColor = currentLine->color;
			currentLine->color = D2D1::ColorF(D2D1::ColorF::White);
			lines.erase(nearL);
		}

		float distance(D2D1_POINT_2F a, D2D1_POINT_2F b, D2D1_POINT_2F c)
		{
			return min(triangleSpace(a, b, c) / distance(a, b),
				min(distance(a, c), distance(b, c)));
		}

		void swap(float&a, float&b)
		{
			a =a+ b;
			b = a - b;
			a = a -b;
		}

		bool inbound(float a, float b, float c, float d)
		{
			if (a > b) swap(a, b);
			if (c > d) swap(c, d);
			return max(a, c) <= min(b, d);
		}
		float signedTriangleSpace(D2D1_POINT_2F a, D2D1_POINT_2F b, D2D1_POINT_2F c)
		{
			return ((a.x - c.x)*(b.y - c.y) - (b.x - c.x)*(a.y - c.y)) / 2;
		}

		bool intersect(D2D1_POINT_2F a, D2D1_POINT_2F b, D2D1_POINT_2F c, D2D1_POINT_2F d)
		{
			return inbound(a.x, b.x, c.x, d.x)
				&& inbound(a.y, b.y, c.y, d.y)
				&& signedTriangleSpace(a, b, c)*signedTriangleSpace(a, b, d) <= 0
				&& signedTriangleSpace(c, d, a)*signedTriangleSpace(c, d, b) <= 0;
		}

		bool checkintersect(Line * l, D2D1_POINT_2F &p)
		{
			int sz = l->points.size();
			if (sz > 2)
			{
				for (int i = 0; i < sz-2; i++)
				{
					if (intersect(l->points[i], l->points[i + 1], l->points[points.size() - 1], p))
						return true;
				}
			}
			return false;
		}

		float triangleSpace(D2D1_POINT_2F a, D2D1_POINT_2F b, D2D1_POINT_2F c)
		{
			return fabs((a.x - c.x)*(b.y - c.y) - (b.x - c.x)*(a.y - c.y)) / 2;
		}

		float distance(D2D1_POINT_2F a, D2D1_POINT_2F b)
		{
			float x = a.x - b.x, y = a.y - b.y;
			return sqrtf(x*x + y * y);
		}

		void Render()
		{
			if (!target) return;
			background = D2D1::ColorF(D2D1::ColorF::DarkGray);
			// всего 4 режима антиалиасинга лучший эффект по умолчанию
			//target->SetAntialiasMode(D2D1_ANTIALIAS_MODE::D2D1_ANTIALIAS_MODE_FORCE_DWORD);
			target->BeginDraw();
			target->Clear(background);
			for (int i = 0; i<lines.size(); i++)
			{
				lines[i]->Render(target);
			}
			currentLine->Render(target);
			if (isChangePoint)
			{
				D2D1_ELLIPSE circle = { *currentpoint,5,5 };
				ID2D1SolidColorBrush *brush;
				target->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::GreenYellow), &brush);
				target->DrawEllipse(circle, brush);
			}

			target->EndDraw();
		}

		void Resize(HWND handle)
		{
			if (!target) return;
			RECT rect;
			if (!GetClientRect(handle, &rect)) return;
			D2D1_SIZE_U size = D2D1::SizeU(rect.right - rect.left, rect.bottom - rect.top);
			target->Resize(size);
		}

	private:

		ID2D1Factory * factory;
		ID2D1HwndRenderTarget* target;
	};
	ID2D1LinearGradientBrush *LBrush;
	ID2D1RadialGradientBrush  *RBrush;
	ID2D1SolidColorBrush *SBrush;
	ID2D1Brush *brush;

	ID2D1GradientStopCollection *pGradientStops = NULL;

	D2D1_GRADIENT_STOP gradientStops[2];

	void Line::Render(ID2D1HwndRenderTarget* target)
	{
		if (points.size()>1)
		{
			if (this->gradient)
			{
				switch (this->style)
				{
				case 1:// linear
					{
						gradientStops[0].color = gc1;
						gradientStops[0].position = 0.0f;
						gradientStops[1].color = gc2;
						gradientStops[1].position = 1.0f;
						HRESULT hr = target->CreateGradientStopCollection(
							gradientStops,
							2,
							D2D1_GAMMA_2_2,
							D2D1_EXTEND_MODE_WRAP,
							&pGradientStops
						);
						if (SUCCEEDED(hr))
						{
							hr = target->CreateLinearGradientBrush(
								D2D1::LinearGradientBrushProperties(
									D2D1::Point2F(0, 0),
									D2D1::Point2F(150, 150)),
								pGradientStops,
								&LBrush
							);
							brush = LBrush;
						}

					}
				case 2:// Radial;
				{
					gradientStops[0].color = gc1;
					gradientStops[0].position = 0.0f;
					gradientStops[1].color = gc2;
					gradientStops[1].position = 1.0f;
					HRESULT hr = target->CreateGradientStopCollection(
						gradientStops,
						2,
						D2D1_GAMMA_2_2,
						D2D1_EXTEND_MODE_WRAP,
						&pGradientStops
					);
					if (SUCCEEDED(hr))
					{
						hr = target->CreateRadialGradientBrush(
							D2D1::RadialGradientBrushProperties(
								D2D1::Point2F(75, 75),
								D2D1::Point2F(0, 0),
								75,
								75),
							pGradientStops,
							&RBrush
						);
						brush = RBrush;
					}
				}

				}
			}
			else
			{ 
				target->CreateSolidColorBrush(color, &SBrush);
				brush = SBrush;
			}
			for (int i = 0; i<points.size() - 1; i++)
				target->DrawLine(points[i], points[i + 1], brush);
		}
	}

	public ref class Scene
	{
	public:

		Scene(System::IntPtr handle)
		{
			renderer = new Renderer;
			if (renderer) renderer->Initialize((HWND)handle.ToPointer());
		}

		~Scene()
		{
			delete renderer;
		}


		void AddPoint(int x, int y)
		{
			if (renderer) renderer->AddPoint(x, y);
		}
		void SetColor(unsigned int color)
		{
			if (renderer) renderer->SetColor(color);
		}
		void SetGradient(unsigned int color1, unsigned int color2, unsigned int style)
		{
			if (renderer) renderer->SetGradient(color1,color2,style);
		}

		void DeletePoly()
		{
			if (renderer) renderer->DeletePoly();
		}
		void DeletePoint()
		{
			if (renderer) renderer->DeletePoint();
		}
		void NewPoly()
		{
			if (renderer) renderer->NewPoly();
		}
		void ChangePoly(int x, int y)
		{
			if (renderer) renderer->ChangePoly(x, y);
		}
		void ChangePoint(int x, int y)
		{
			if (renderer) renderer->ChangePoint(x, y);
		}
		void RestorePolyColor()
		{
			if (renderer) renderer->RestorePolyColor();
		}
		void Resize(System::IntPtr handle)
		{
			HWND hwnd = (HWND)handle.ToPointer();
			if (renderer) renderer->Resize(hwnd);
		}

		void Draw()
		{
			if (renderer) renderer->Render();
		}

	private:

		Renderer * renderer;
	};
}