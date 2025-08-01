import { Component, inject } from '@angular/core';
import { CommonModule, NgClass } from "@angular/common";
import { FormsModule } from "@angular/forms";
import {DocumentsService, UpdateDocumentStatusDto} from "../../services/documents.service";
import { AuthService } from "../../services/auth.service";
import { Router } from "@angular/router";
import { DocumentFile } from "../dashboard/dashboard.component";
import { CommentsService, Comment as CommentModel } from "../../services/comments.service";
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

@Component({
  selector: 'app-documents',
  standalone: true,
  imports: [
    NgClass,
    FormsModule,
    CommonModule
  ],
  templateUrl: './documents.component.html',
  styleUrls: ['./documents.component.css']
})
export class DocumentsComponent {
  isSidebarOpen = true;
  files: DocumentFile[] = [];
  searchText: string = '';
  selectedType: string = '';
  selectedStatus: string = '';
  fromDate: string = '';  // ISO format yyyy-MM-dd
  toDate: string = '';
  selectedDate: string = '';

  private documentsService = inject(DocumentsService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private commentsService = inject(CommentsService);
  private http = inject(HttpClient);
  private sanitizer = inject(DomSanitizer);

  statusMap: { [key: number]: string } = {
    0: 'Draft',
    1: 'WaitingApproval',
    2: 'Approved',
    3: 'ReturnedForEdit',
    4: 'Archived'
  };

  snackbarMessage: string = '';
  showSnackbar: boolean = false;

  showSuccessSnackbar(message: string) {
    this.snackbarMessage = message;
    this.showSnackbar = true;

    setTimeout(() => {
      this.showSnackbar = false;
    }, 3000);
  }
  ngOnInit() {
    this.loadDocuments();
  }

  sortOption: string = 'date-desc'; // podrazumevano sortiranje

  loadDocuments() {
    const [sortBy, order] = this.sortOption.split('-');
    const hasAdvancedFilters = this.selectedType || this.selectedStatus || this.fromDate || this.toDate;

    if (this.searchText && this.searchText.trim() !== '') {
      this.documentsService.searchDocuments({
        query: this.searchText.trim(),
        sortBy,
        order
      }).subscribe({
        next: (docs) => this.files = docs.map(doc => this.mapDocToFile(doc)),
        error: (err) => console.error('Error searching documents:', err)
      });
    } else if (hasAdvancedFilters) {
      const params: any = { sortBy, order };
      if (this.selectedType) params.documentType = this.selectedType;
      if (this.selectedStatus) params.status = this.selectedStatus;
      if (this.fromDate) params.fromDate = this.fromDate;
      if (this.toDate) params.toDate = this.toDate;

      this.documentsService.searchAdvanced(params).subscribe({
        next: (docs) => this.files = docs.map(doc => this.mapDocToFile(doc)),
        error: (err) => console.error('Error loading advanced filtered documents:', err)
      });
    } else {
      this.documentsService.sortDocuments(sortBy, order).subscribe({
        next: (docs) => this.files = docs.map(doc => this.mapDocToFile(doc)),
        error: (err) => console.error('Error loading sorted documents:', err)
      });
    }
  }

  private mapDocToFile(doc: any): DocumentFile {
    return {
      id: doc.id,
      name: doc.fileName,
        project: doc.project,
      description: doc.description,
      timestamp: new Date(doc.uploadedAt).toLocaleDateString(),
      author: doc.uploadedBy,
      version: doc.version,
      status: this.statusMap[doc.status] || 'Unknown',
      type: doc.documentType
    };
  }

  handleDocumentsResponse(docs: any[]) {
    this.files = docs.map(doc => ({
      id: doc.id,
      name: doc.fileName,
      description: doc.description,
      timestamp: new Date(doc.uploadedAt).toLocaleDateString(),
      author: doc.uploadedBy,
      version: doc.version,
      status: this.statusMap[doc.status] || 'Unknown',
      type: doc.documentType
    })) as DocumentFile[];
  }

  showCommentsDialog = false;
  comments: CommentModel[] = [];
  selectedDocumentId: number | null = null;

  openComments(documentId: number) {
    this.selectedDocumentId = documentId;
    this.loadComments(documentId);
    this.showCommentsDialog = true;
  }

  closeComments() {
    this.showCommentsDialog = false;
    this.comments = [];
    this.selectedDocumentId = null;
  }

  loadComments(documentId: number) {
    this.commentsService.getCommentsForDocument(documentId)
      .subscribe({
        next: (data) => this.comments = data,
        error: (err) => {
          console.error('Error loading comments', err);
          this.comments = [];
        }
      });
  }

  toggleSidebar() {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  showUploadDialog = false;
  uploadData = {
    project: '',
    description: ''
  };
  selectedFile: File | null = null;

  onUploadClick(): void {
    this.showUploadDialog = true;
  }

  closeUploadDialog(): void {
    this.showUploadDialog = false;
    this.uploadData = { project: '', description: '' };
    this.selectedFile = null;
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
    }
  }

  uploadDocument(): void {
    if (!this.selectedFile) {
      alert("Please select a file.");
      return;
    }

    const formData = new FormData();
    formData.append('file', this.selectedFile);
    formData.append('project', this.uploadData.project);
    formData.append('description', this.uploadData.description);

    this.documentsService.uploadDocument(formData).subscribe({
      next: (response) => {
        //alert('Upload successful! Version: ' + response.version);
        this.showSuccessSnackbar('Upload successful! Version: ' + response.version);
        this.closeUploadDialog();
        this.loadDocuments();
      },
      error: (err) => {
        console.error('Upload failed', err);
        //alert('Upload failed.');
        this.showSuccessSnackbar('Upload failed.');
      }
    });
  }

  showDocumentDialog = false;
  dialogFileUrl: SafeResourceUrl | null = null;
  dialogType: string | null = null;
  newCommentContent: any;
  selectedStatusForComment: any;
  showCommentInputDialog: any;
  commentTargetDocumentId: any;
  newCommentText: any;
  newCommentStatus: any;

  openDocumentDialog(file: DocumentFile) {
    this.dialogType = file.type.toLowerCase();

    const token = this.authService.getToken();
    if (!token) {
      console.error('No JWT token found!');
      return;
    }

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    this.http.get(`https://localhost:7053/api/Documents/${file.id}/stream`, {
      headers,
      responseType: 'blob'
    }).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);

        if (this.dialogType === 'pdf') {
          this.dialogFileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(url);
        } else if (this.dialogType === 'docx') {
          const googleDocsUrl = `https://docs.google.com/gview?url=${encodeURIComponent(url)}&embedded=true`;
          this.dialogFileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(googleDocsUrl);
        } else {
          // Za DWG i ostale formate
          this.dialogFileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(url);
        }

        this.showDocumentDialog = true;
      },
      error: (err) => {
        console.error('Error loading document blob:', err);
      }
    });
  }



  closeDocumentDialog() {
    this.showDocumentDialog = false;
    this.dialogFileUrl = null;
    this.dialogType = null;
  }


  openCommentInputDialog(documentId: number) {
    this.commentTargetDocumentId = documentId;
    this.selectedDocumentId = documentId;
    this.newCommentText = '';
    this.newCommentStatus = '';
    this.showCommentInputDialog = true;
  }


  closeCommentInputDialog() {
    this.showCommentInputDialog = false;
    this.commentTargetDocumentId = null;
    this.newCommentText = '';
    this.newCommentStatus = '';
  }
  statusMap2: { [key: number]: string } = {
    0: 'Draft',
    1: 'WaitingApproval',
    2: 'Approved',
    3: 'ReturnedForEdit',
      4: 'Archived'
  };

  statusStringToNumberMap: { [key: string]: number } = {
    'Draft': 0,
    'WaitingApproval': 1,
    'Approved': 2,
    'ReturnedForEdit': 3,
      'Archived': 4
  };

  updateStatusAndComment(documentId: number, commentContent: string, status?: string) {
    if (!commentContent?.trim() && !status) {
      //alert('You must provide a comment or select a status.');
      this.showSuccessSnackbar('You must provide a comment or select a status!');
      return;
    }

    const dto: UpdateDocumentStatusDto = {
      commentContent: commentContent.trim(),
    };

    if (status) {
      const statusNum = this.statusStringToNumberMap[status];
      if (statusNum === undefined) {
        //alert('Invalid status selected.');
        this.showSuccessSnackbar('Invalid status selected.');
        return;
      }
      dto.status = statusNum;
    }

    this.documentsService.updateDocumentStatus(documentId, dto).subscribe({
      next: (res) => {
        //alert('Successfully updated comment and/or status.');
        this.showSuccessSnackbar('Successfully updated comment and/or status.');
        this.closeComments();
        this.loadDocuments();
      },
      error: (err) => {
        console.error('Error updating status/comment:', err);
        //alert('Failed to update status or add comment.');
        this.showSuccessSnackbar('Failed to update status or add comment.');
      }
    });
  }

  downloadDocument(file: DocumentFile) {
    const token = this.authService.getToken();
    if (!token) {
      console.error('No JWT token found!');
      return;
    }

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    this.http.get(`https://localhost:7053/api/Documents/${file.id}/stream`, {
      headers,
      responseType: 'blob'
    }).subscribe({
      next: (blob) => {
        const blobUrl = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = blobUrl;
        a.download = file.name;  // npr. "myfile.pdf"
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(blobUrl); // oslobodi memoriju
      },
      error: (err) => {
        console.error('Error downloading file:', err);
      }
    });
  }



}


