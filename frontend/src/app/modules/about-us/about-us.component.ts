import { Component } from '@angular/core';
import {NgForOf, NgIf} from "@angular/common";
import { HttpClient } from '@angular/common/http';
import {RouterLink} from "@angular/router";
import {UserEndpointsService, UserReadResponse} from "../../endpoints/UserEndpointsService";
import {MyAuthService} from "../../services/auth-services/my-auth.service";
import * as d3 from 'd3';
@Component({
  selector: 'app-about-us',
  standalone: true,
  imports: [
    NgIf,
    NgForOf,
    RouterLink
  ],
  templateUrl: './about-us.component.html',
  styleUrls: ['./about-us.component.css']
})
export class AboutUsComponent {
  images: string[] = [];
  chartData: { date: string; count: number }[] = [];
  zoomScale: number = 1;
  minZoomScale: number = 1;
  maxZoomScale: number = 3;
  isLoggedIn: boolean = false;
  user: UserReadResponse | null = null;
  isShelter:boolean=false;
  isUser:boolean=false;

  constructor(private http:HttpClient,
              private authService: MyAuthService, private userService: UserEndpointsService) {
  }


  ngOnInit(): void {
    this.fetchShelterImages();

    this.fetchAdoptionStatistics();
    this.isLoggedIn = this.authService.isLoggedIn();
    const role=this.authService.getCurrentUser()?.Role;
    if(role=='Shelter')
    {
      this.isShelter=true;
    }
    else {
      this.isUser=true;
    }
    if(this.isLoggedIn) {
      this.userService.loadUserProfile().subscribe({
        next: (data: UserReadResponse) => {
          this.user = data;
        },
        error: (error) => {
          console.error('Greška pri preuzimanju korisniĝkih podataka:', error);
        }
      });
    }
  }

  isModalOpen = false;
  currentImageIndex = 0;


  fetchAdoptionStatistics(): void {
    this.http.get<{ date: string; count: number }[]>(`https://localhost:7291/adoption-statistics`).subscribe({
      next: (data) => {

        const formattedData = data.map(d => {
          const date = new Date(d.date);
          const formattedDate = `${date.getFullYear()}-${(date.getMonth() + 1)
            .toString()
            .padStart(2, '0')}-${date.getDate().toString().padStart(2, '0')}`;
          return { date: formattedDate, count: d.count };
        });


        const today = new Date();
        const last7Days = [];
        for (let i = 6; i >= 0; i--) {
          const date = new Date(today);
          date.setDate(today.getDate() - i);
          const formattedDate = `${date.getFullYear()}-${(date.getMonth() + 1)
            .toString()
            .padStart(2, '0')}-${date.getDate().toString().padStart(2, '0')}`;

          const entry = formattedData.find(d => d.date === formattedDate);
          last7Days.push(entry || { date: formattedDate, count: 0 });
        }


        this.chartData = last7Days;


        this.createChart();
      },
      error: (err) => {
        console.error('Greška pri preuzimanju statistike udomljenja:', err);
      }
    });
  }


  createChart(): void {
    const data = this.chartData;

    const svg = d3
      .select('#chart')
      .append('svg')
      .attr('width', '100%')
      .attr('height', 450);

    const margin = { top: 60, right: 30, bottom: 130, left: 70 };
    const svgElement = svg.node();
    if (svgElement) {
      const width = svgElement.getBoundingClientRect().width - margin.left - margin.right;
      const height = svgElement.getBoundingClientRect().height - margin.top - margin.bottom;


      svg.append('text')
        .attr('x', width / 2 + margin.left)
        .attr('y', margin.top / 2)
        .attr('text-anchor', 'middle')
        .style('font-size', '20px')
        .style('fill', '#8B4513')
        .style('opacity', 0)
        .transition()
        .duration(1000)
        .style('opacity', 1)
        .text('Broj udomljenih ljubimaca u zadnjih sedam dana');

      const x = d3.scaleBand()
        .domain(data.map(d => d.date))
        .range([0, width])
        .padding(0.2);

      const y = d3.scaleLinear()
        .domain([0, d3.max(data, (d) => d.count) || 0])
        .nice()
        .range([height, 0]);

      const g = svg.append('g')
        .attr('transform', `translate(${margin.left},${margin.top})`);

      const bars = g.append('g')
        .selectAll('.bar')
        .data(data)
        .enter()
        .append('rect')
        .attr('class', 'bar')
        .attr('x', (d) => x(d.date) ?? 0)
        .attr('y', height)
        .attr('width', x.bandwidth() * 0.8)
        .attr('height', 0)
        .attr('fill', '#D2B48C')
        .style('transition', 'all 0.5s ease-out');

      bars.transition()
        .duration(1000)
        .delay((_, i) => i * 200)
        .attr('y', (d) => y(d.count))
        .attr('height', (d) => height - y(d.count));

      const formatDate = d3.timeFormat('%d %m %Y');

      const xAxis = d3.axisBottom(x)
        .tickFormat(d => formatDate(new Date(d)))
        .tickSize(6);

      g.append('g')
        .attr('class', 'x-axis')
        .attr('transform', `translate(0,${height})`)
        .call(xAxis)
        .selectAll('text')
        .style('font-size', '16px')
        .style('fill', '#8B4513')
        .style('text-anchor', 'middle');

      svg.append('text')
        .attr('class', 'x-axis-label')
        .attr('transform', `translate(${width / 2 + margin.left}, ${height + margin.top + 55})`)
        .style('text-anchor', 'middle')
        .style('font-size', '14px')
        .style('fill', '#8B4513')
        .text('Datum');
      const maxCount = d3.max(data, (d) => d.count) || 0;
      const yAxis = d3.axisLeft(y)
        .tickFormat(d3.format('d'))
        .tickValues(d3.range(0, maxCount + 1))
        .tickSize(6);

      g.append('g')
        .attr('class', 'y-axis')
        .call(yAxis)
        .selectAll('text')
        .style('font-size', '16px')
        .style('fill', '#8B4513')
        .style('text-anchor', 'middle');

      g.append('text')
        .attr('class', 'y-label')
        .attr('transform', 'rotate(-90)')
        .attr('x', -height / 2)
        .attr('y', -margin.left + 15)
        .style('text-anchor', 'middle')
        .style('font-size', '14px')
        .style('fill', '#8B4513')
        .text('Broj usvojenih životinja');

      g.append('g')
        .selectAll('.grid')
        .data(y.ticks(5))
        .enter()
        .append('line')
        .attr('class', 'grid')
        .attr('x1', 0)
        .attr('x2', width)
        .attr('y1', (d) => y(d))
        .attr('y2', (d) => y(d))
        .attr('stroke', '#D2B48C')
        .attr('stroke-width', 0.5);

      bars.on('mouseover', function (event, d) {
        d3.select(this)
          .transition()
          .duration(200)
          .attr('fill', '#A0522D')
          .attr('height', height - y(d.count) + 10)
          .attr('y', y(d.count) - 5);
      })
        .on('mouseout', function (event, d) {
          d3.select(this)
            .transition()
            .duration(200)
            .attr('fill', '#D2B48C')
            .attr('height', height - y(d.count))
            .attr('y', y(d.count));
        });
    }
  }

  fetchShelterImages(): void {
    this.http.get<string[]>(`https://localhost:7291/shelters/images`).subscribe({
      next: (images) => {
        this.images = images;
        console.log('Fetched images:', images);
      },
      error: (err) => {
        console.error('Error fetching shelter images:', err);
      },
    });
  }

  openModal(index: number): void {
    this.currentImageIndex = index;
    this.isModalOpen = true;
  }

  closeModal(): void {
    this.isModalOpen = false;
  }

  prevImage(): void {
    this.currentImageIndex =
      (this.currentImageIndex - 1 + this.images.length) % this.images.length;
  }

  nextImage(): void {
    this.currentImageIndex =
      (this.currentImageIndex + 1) % this.images.length;
  }




  setZoomLevel(scale: number): void {

    this.zoomScale = Math.min(this.maxZoomScale, Math.max(this.minZoomScale, scale));

    const image = document.querySelector('.zoomable-modal-image') as HTMLImageElement;
    if (image) {
      image.style.transform = `scale(${this.zoomScale})`;
    }
  }

  onWheelZoom(event: WheelEvent): void {
    const zoomFactor = event.deltaY < 0 ? 0.1 : -0.1;
    this.setZoomLevel(this.zoomScale + zoomFactor);
    event.preventDefault();
  }

}

