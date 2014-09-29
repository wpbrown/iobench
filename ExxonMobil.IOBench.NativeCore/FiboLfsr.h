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
#pragma once

#include <cstdint>

class FiboLfsr
{
public:
	FiboLfsr();
	FiboLfsr(uint8_t width);
	~FiboLfsr();

	uint32_t Next();

private:
	struct Polynomial {
		uint8_t width;
		uint8_t shifts[4]; // taps
		bool doubleTap; // zombieland rule #2
	};

	static const Polynomial polys[15];

	const Polynomial* poly;
	uint32_t lfsr;
	uint32_t seed;
	bool complete;
};

