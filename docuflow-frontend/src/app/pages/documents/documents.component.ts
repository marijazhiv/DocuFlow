import { Component } from '@angular/core';
import {CommonModule, NgClass} from "@angular/common";
import {FormsModule} from "@angular/forms";

interface File {
  name: string;
  description: string;
  timestamp: string; // format: "YYYY-MM-DD" ili "YYYY-MM-DDTHH:mm:ss"
  author: string;
  version: string;
  status: string;
  type: string; // za filter (npr. 'report', 'invoice', 'contract')
}
@Component({
  selector: 'app-documents',
  standalone: true,
  imports: [
    NgClass,
    FormsModule,
     CommonModule
  ],
  templateUrl: './documents.component.html',
  styleUrl: './documents.component.css'
})
export class DocumentsComponent {
  isSidebarOpen = true;

  searchText: string = '';
  selectedType: string = '';
  selectedDate: string = '';
  sortOption: string = 'name';

  files: File[] = [
    {
      name: 'Annual Report 2024',
      description: 'Company annual financial report.',
      timestamp: '2025-07-01',
      author: 'John Doe',
      version: '1.0',
      status: 'active',
      type: 'report'
    },
    {
      name: 'Invoice #12345',
      description: 'Invoice for client ABC Corp.',
      timestamp: '2025-06-15',
      author: 'Jane Smith',
      version: '1.2',
      status: 'draft',
      type: 'invoice'
    },
    {
      name: 'Contract with Supplier',
      description: 'Signed contract agreement.',
      timestamp: '2025-05-20',
      author: 'Michael Johnson',
      version: '2.0',
      status: 'archived',
      type: 'contract'
    },
    {
      name: 'Invoice #12345',
      description: 'Invoice for client ABC Corp.',
      timestamp: '2025-06-15',
      author: 'Jane Smith',
      version: '1.2',
      status: 'draft',
      type: 'invoice'
    }
    // Dodaj još fajlova po potrebi
  ];

  get filteredFiles(): File[] {
    let files = this.files;

    if (this.searchText) {
      files = files.filter(f =>
        f.name.toLowerCase().includes(this.searchText.toLowerCase())
      );
    }

    if (this.selectedType) {
      files = files.filter(f => f.type === this.selectedType);
    }

    if (this.selectedDate) {
      // Provera samo početka datuma, možeš prilagoditi po potrebi
      files = files.filter(f => f.timestamp.startsWith(this.selectedDate));
    }

    switch (this.sortOption) {
      case 'name':
        files = files.sort((a, b) => a.name.localeCompare(b.name));
        break;
      case 'date':
        files = files.sort((a, b) => b.timestamp.localeCompare(a.timestamp));
        break;
      case 'author':
        files = files.sort((a, b) => a.author.localeCompare(b.author));
        break;
    }

    return files;
  }

  toggleSidebar() {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  showUploadDialog = false;

  uploadData = {
    project: '',
    description: '',
    file: null as File | null
  };

  onUploadClick() {
    this.showUploadDialog = true;
  }

  closeUploadDialog() {
    this.showUploadDialog = false;
    this.uploadData = { project: '', description: '', file: null };
  }

  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    if (file) {
      this.uploadData.file = file;
    }
  }

  uploadDocument() {
    if (!this.uploadData.project || !this.uploadData.file) {
      alert('Please fill in project and select a file.');
      return;
    }
    // Upload logic ovde...

    alert(`Uploading document: ${this.uploadData.project}, file: ${this.uploadData.file.name}`);
    this.closeUploadDialog();
  }
}
