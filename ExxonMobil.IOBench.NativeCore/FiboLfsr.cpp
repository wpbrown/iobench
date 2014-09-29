// Copyright 2014 ExxonMobil Technical Computing Company
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#include "stdafx.h"

#include "FiboLfsr.h"

#include <exception>

const FiboLfsr::Polynomial FiboLfsr::polys[15] = {
	{ 2, {0, 1, 2, 2}, false}, // degrees: 2, 1, 0, 0
	{ 3, {0, 1, 3, 3}, false}, // degrees: 3, 2, 0, 0
	{ 4, {0, 1, 4, 4}, false}, // degrees: 4, 3, 0, 0
	{ 5, {0, 2, 5, 5}, false}, // degrees: 5, 3, 0, 0
	{ 6, {0, 1, 6, 6}, false}, // degrees: 6, 5, 0, 0
	{ 7, {0, 1, 7, 7}, false}, // degrees: 7, 6, 0, 0
	{ 8, {0, 2, 3, 4}, true}, // degrees: 8, 6, 5, 4
	{ 9, {0, 4, 9, 9}, false}, // degrees: 9, 5, 0, 0
	{ 10, {0, 3, 10, 10}, false}, // degrees: 10, 7, 0, 0
	{ 11, {0, 2, 11, 11}, false}, // degrees: 11, 9, 0, 0
	{ 12, {0, 1, 2, 8}, true}, // degrees: 12, 11, 10, 4
	{ 13, {0, 1, 2, 5}, true}, // degrees: 13, 12, 11, 8
	{ 14, {0, 1, 2, 12}, true}, // degrees: 14, 13, 12, 2
	{ 15, {0, 1, 15, 15}, false}, // degrees: 15, 14, 0, 0
	{ 16, {0, 2, 3, 5}, true} // degrees: 16, 14, 13, 11
};

FiboLfsr::FiboLfsr() :
	complete(true)
{
}

FiboLfsr::FiboLfsr(uint8_t width) :
	complete(false)
{
	if (width < 2 || width > 16)
		throw std::exception("Width out of range (2-16)");

	poly = polys + (width - 2);

	uint32_t mask = (UINT32_MAX >> (32 - width));
	lfsr = seed = 0xBEEF & mask;
}

FiboLfsr::~FiboLfsr()
{
}

uint32_t FiboLfsr::Next()
{
	if (complete) 
		return 0;

	uint32_t bit;
	const uint8_t* shifts = poly->shifts;

	if (poly->doubleTap)
		bit  = ((lfsr >> shifts[0]) ^ (lfsr >> shifts[1]) ^ (lfsr >> shifts[2]) ^ (lfsr >> shifts[3])) & 1;
	else
		bit  = ((lfsr >> shifts[0]) ^ (lfsr >> shifts[1])) & 1;

	lfsr = (lfsr >> 1) | (bit << (poly->width - 1));

	if (lfsr == seed)
		complete = true;

	return lfsr;
}

